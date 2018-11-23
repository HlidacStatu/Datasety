const Apify = require('apify');
const request = require('request-promise');

// send one item to Hlidac Statu (HS doesn't provide API for batch import)
const pushItemToHS = async (item) => {
    try {
        const response = await request({
            url: "https://www.hlidacstatu.cz/api/v1/DatasetItem/seznam-politickych-stran/" + item.id,
            body: item,
            method: "POST",
            json: true,
            headers: {
                Authorization: "Token " + process.env.HS_TOKEN
            }
        })
        console.log("Data imported. Response:" + JSON.stringify(response, null, 2));
    } catch (e){
        console.log(e);
    }
};

// extract all data from detail page (party deatils + people)
const extractDataFromPage = async (page) => {
    await Apify.utils.puppeteer.injectJQuery(page);
    const result = await page.evaluate(() => {
        
        const parseDate = (date) => {
            // 12.9.1954 => 1954-09-12T00:00Z
            const dateParts = date.split('.');
            const month = dateParts[1].length === 1 ? "0" + dateParts[1] : dateParts[1];
            const day = dateParts[0].length === 1 ? "0" + dateParts[0] : dateParts[0];
            return dateParts[2] + "-" + month + "-" + day;// + "T00:00Z";
        }

        const people = [];
        // find and iterate all people
        $('h3:contains("Osoby")').closest('tr').nextUntil($('h3').closest('tr')).each(function() {
            const data = $(this).find('td:eq(1)').text().trim().split('\n');
            try{
                people.push({
                    jmeno: data[0].trim(),
                    narozeni: parseDate(data[1].trim()),
                    adresa: data[2].trim() + ", " + data[3].trim(),
                    platiOd: parseDate(data[4].trim().replace("Platí od: ","")),
                    funkce: $(this).find('td:eq(0)').text().trim(),
                    HsProcessType: "person"
                })
            } catch (e){
                console.log(e);
            }
        });
        
        // extract main attributes
        return {
            nazev: $('#ctl00_Application_lblNazevStrany').text().trim(),
            zkratka: $('#ctl00_Application_lblZkratkaStrany').text().trim(),
            adresaSidla: $('#ctl00_Application_lblAdresaSidla').text().trim(),
            denRegistrace: parseDate($('#ctl00_Application_lblDenRegistrace').text().trim()),
            cisloRegistrace: $('#ctl00_Application_lblCisloRegistrace').text().trim(),
            identifikacniCislo: $('#ctl00_Application_lblIdentCislo').text().trim(),
            statutarniOrgan: $('#ctl00_Application_lblStatutarOrgan').text().trim(),
            id: $('#ctl00_Application_lblIdentCislo').text().trim(),
            url: window.location.href,
            osoby: people
        }
    })
    
    return result;
};

Apify.main(async () => {
    // Collect the data to array and push them to dataset at the end.
    const data = [];
    
    const queue = await Apify.openRequestQueue();
    const browser = await Apify.launchPuppeteer({ liveView: true, dumpio: true });
    
    // Go to search.
    const page = await browser.newPage();
    await page.goto('https://aplikace.mvcr.cz/seznam-politickych-stran/Default.aspx');
    console.log('Clicking search ...');
    // await page.click('input[value="Vyhledat"]');
    await page.click('input[value="Vyhledat všechny strany a hnutí"]');

    // Iterate over pagination.
    let finished = false;
    while (!finished) {  
        console.log('Waiting for search results ...');
        await page.waitForSelector('#searchResults');
        
        // Get search result items.
        const linkCount = await page.$$eval('#searchResults a', $links => $links.length);
        console.log(`${linkCount} items found`);
        
        // Iterate all items at current page.
        for (let i = 0; i < linkCount; i++) {
            console.log(`Clicking item ${i+1}/${linkCount}`);
            
            // Click i-th link.
            const links = await page.$$('#searchResults a');
            await links[i].click();
            await page.waitForSelector('#ctl00_Application_lblNazevStrany');

            // Extract data.
            const extracted = await extractDataFromPage(page);
            console.log(extracted);
            data.push(extracted);
            
            // Send data to Hlidac Statu
            await pushItemToHS(extracted);
            
            // Go back.
            await page.goBack();
        }
 
        // Go to next page of results.
        console.log(`Going to the next page of results ...`);
        // Next page link is always located after the .currentPage item.
        // If that item is not link then we return true which means we are done.
        finished = await page.evaluate(() => {
            const maybeNextPageLink = document.getElementsByClassName('currentPage')[0].nextElementSibling;
            const isLink = maybeNextPageLink && maybeNextPageLink.href;
            if (isLink) maybeNextPageLink.click();
    
            return !isLink;
        });

        // Wait  a bit for redirect to be triggered.
        await Apify.utils.sleep(1000);
    }
    
    console.log('Outputting the data ...');
    await Apify.pushData(data);
});
