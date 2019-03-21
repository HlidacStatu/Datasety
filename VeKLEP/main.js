const Apify = require('apify');
const request = require('request-promise');

// send one item to Hlidac Statu (HS doesn't provide API for batch import)
const pushItemToHS = async (item) => {
    try {
        const response = await request({
            url: "https://www.hlidacstatu.cz/api/v1/DatasetItem/veklep/" + item.Id,
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

// helper function to navigate on a website - click and wait for a selector
const clickAndWait = async (page, selector) => {
    await page.waitForSelector(selector);
    const promise = page.waitForNavigation();
    await page.click(selector);
    await promise;
    await Apify.utils.puppeteer.injectJQuery(page);
};

// extract all data from detail page, first tab
const extractDataFromFirstTab = async (page) => {
    const result = await page.evaluate(() => {  
        // extract main attributes   
        const data = {
            Id: $('th:contains("PID")').next('td').text().trim(),
            cjOva: $('th:contains("Čj. OVA")').next('td').text().trim(),
            PID: $('th:contains("PID")').next('td').text().trim(),
            cjPredkladatele: $('th:contains("Čj. předkladatele")').next('td').text().trim(),
            datumAutorizace: window.apifyParseDate($('th:contains("Datum autorizace")').next('td').text().trim()),
            nazevMaterialu: $('th:contains("Název materiálu")').next('td').text().trim(),
            typMaterialu: $('th:contains("Typ materiálu")').next('td').text().trim(),
            predkladatel: $('th:contains("Předkladatel")').next('td').text().trim(),
            klicovaSlova: $('th:contains("Klíčová slova")').next('td').text().trim(),
            oblastPrava: $('th:contains("Oblast práva")').next('td').text().trim(),
            duvodPredlozeni: $('th:contains("Důvod předložení")').next('td').text().trim(),
            popis: $('th:contains("Popis")').next('td').text().trim(),
            poznamky: $('th:contains("Poznámky")').next('td').text().trim(),
            datumPosledniUpravy: window.apifyParseDate($('th:contains("Datum poslední úpravy")').next('td').text().trim()),
            datumSchuzeVlady: window.apifyParseDate($('th:contains("Datum schůze vlády")').next('td').text().trim()),
            cisloSnemovnihoTisku: $('th:contains("Číslo Sněmovního tisku")').next('td').text().trim(),
            legislativniProcesPokracujeDo: $('th:contains("Legislativní proces pokračuje do")').next('td').text().trim(),
            adresaPripominek: $('th:contains("Adresa připomínek")').next('td').text().trim(),
            pripominkovaMista: $('th:contains("Připomínková místa")').next('td').text().trim().split(/[,;]+/).map(function(item) {
                    return item.trim();
                }),
            prilohy: [],
            url: window.location.href
        }
        const terminPripominek = $('th:contains("Termín připomínek")').next('td').text().trim().split('–');
        if (terminPripominek.length === 2) {
            data.terminPripominekOdData = window.apifyParseDate(terminPripominek[0].trim());
            data.terminPripominekDoData = window.apifyParseDate(terminPripominek[1].trim());
        }
    
        const stavMaterialu = $('th:contains("Stav materiálu")').next('td').text().trim().split('-');
        if (stavMaterialu.length === 2) {
            data.stavMaterialuVal = stavMaterialu[0].trim();
            data.stavMaterialuText = stavMaterialu[1].trim();
        }

        $(".material-header:contains('Přílohy')").next('table').find('tr').each(function(){
            if ($(this).find('td').length) {
                data.prilohy.push({
                    typ: $(this).find('td:eq(0)').text().trim(),
                    nazevPrilohy: $(this).find('td:eq(1)').text().trim(),
                    DocumentUrl: "https://apps.odok.cz" + $(this).find('td:eq(1) a').attr('href'),
                    HsProcessType: "document",
                    DocumentPlainText: null,
                    velikostPrilohy: $(this).find('td:eq(2)').text().trim(),
                    datumVlozeniPrilohy: window.apifyParseDate($(this).find('td:eq(3)').text().trim())
                });
            }
        });
        return data;
    })
    return result;
};

// extract all data from detail page, second tab
const extractDataFromSecondTab = async (page) => {
    const result = await page.evaluate(() => {
        const data = { pripominky: [] };
        $(".lfr-table").find('tr').each(function(){
            if ($(this).find('td').length) {
                const item = {
                    dne: window.apifyParseDateTime($(this).find('td:eq(0)').text().trim()),
                    typ: $(this).find('td:eq(1) div').attr('title'),
                    pripominkoveMisto: $(this).find('td:eq(2)').text().trim(),
                    prilohy: [],
                    poznamka: $(this).find('td:eq(6)').text().trim()
                };
                $(this).find('td:eq(4) a').each(function(){
                    item.prilohy.push({
                        nazevPrilohy: $(this).text().trim(),
                        DocumentUrl: "https://apps.odok.cz" + $(this).attr('href'),
                        HsProcessType: "document",
                        DocumentPlainText: null
                    });
                });
                data.pripominky.push(item);
            }
        });
        return data
    })
    return result;
};

// extract all data from detail page, third tab
const extractDataFromThirdTab = async (page) => {
    const result = await page.evaluate(() => {
        const data = { verzeMaterialu: [] };
        // extract main attributes
        $(".lfr-table").find('tr').each(function(){
            if ($(this).find('td').length) {
                data.verzeMaterialu.push({
                    autorizovano: window.apifyParseDate($(this).find('td:eq(0)').text().trim()),
                    popisVerze: $(this).find('td:eq(1)').text().trim(),
                    urlVerze: "https://apps.odok.cz" + $(this).find('td:eq(1) a').attr('href')
                });
            }
        });
        return data;
    })
    return result;
};

Apify.main(async () => {
    
    // Apify.openRequestQueue() is a factory to get a preconfigured RequestQueue instance.
    const requestQueue = await Apify.openRequestQueue();
    
    const startUrl = 'https://apps.odok.cz/veklep-vyhledavani';
    await requestQueue.addRequest(new Apify.Request({ url: startUrl }));
    
    // Create an instance of the PuppeteerCrawler class - a crawler
    // that automatically loads the URLs in headless Chrome / Puppeteer.
    const crawler = new Apify.PuppeteerCrawler({
        requestQueue,
        launchPuppeteerOptions: {},
        // higher limit as we navigte through tabs
        handlePageTimeoutSecs: 600,
        maxConcurrency: 5,

        handlePageFunction: async ({ request, page }) => {
            await Apify.utils.puppeteer.injectJQuery(page);
            console.log(`Processing ${request.url}...`);

            // click on "Hledat"
            if (request.url === startUrl) {
                await clickAndWait(page, "#_search_WAR_odokkpl_searchBtn");
                request.userData.label = 'PAGINATION';    
            }
            
            // detail page to be scraped
            if (request.userData.label === 'DETAIL') {

                // extract main attributes
                const extracted = await extractDataFromFirstTab(page);

                // check if there is a second tab and set id to the element so we can click on it 
                const secondTab = await page.evaluate(() => {
                    $("a.aui-tab-label:contains('Připomínky')").attr("id","secondTab");
                    return $("a.aui-tab-label:contains('Připomínky')").length;
                })

                if (secondTab) {
                    // click on a second tab
                    await clickAndWait(page, "#secondTab");
                    // extract data from the second tab
                    const extracted2 = await extractDataFromSecondTab(page);
                    // merge data
                    Object.assign(extracted, extracted2);
                }

                // check if there is a third tab and set id to the element so we can click on it 
                const thirdTab = await page.evaluate(() => {
                    $("a.aui-tab-label:contains('Verze materiálu')").attr("id","thirdTab");
                    return $("a.aui-tab-label:contains('Verze materiálu')").length;
                })

                if (thirdTab) {
                    // click on a third tab
                    await clickAndWait(page, "#thirdTab");
                    // extract data from the third tab
                    const extracted3 = await extractDataFromThirdTab(page);
                    // merge data
                    Object.assign(extracted, extracted3);
                }

                // push data to dataset
                await Apify.pushData(extracted);
                // send data to Hlidac Statu
                await pushItemToHS(extracted);
            } 
            
            // page with listings
            if (request.userData.label === 'PAGINATION') {

                // enqueue detail pages
                await Apify.utils.enqueueLinks({
                    page,
                    requestQueue,
                    selector: '.results-grid .results-row a',
                    userData: {label: 'DETAIL'}
                });

                // enqueue next page if exists
                const infos = await Apify.utils.enqueueLinks({
                    page,
                    requestQueue,
                    selector: '.page-links .next',
                    userData: {label: 'PAGINATION'}
                });

                if (infos.length === 0) console.log(`${request.url} is the last page!`);
                
            } 
        },

        // This function is called if the page processing failed more than maxRequestRetries+1 times.
        handleFailedRequestFunction: async ({ request }) => {
            console.log(`Request ${request.url} failed too many times`);
        },
        
        gotoFunction: async ({ request, page }) => {
            
            // helper functions to be used in a context of a loaded page
            await page.evaluateOnNewDocument(()=>{
                const parseDate = (date) => {
                    // 12.9.1954 => 1954-09-12
                    const dateParts = date.split('.');
                    if (dateParts.length != 3) return null;
                    const month = dateParts[1].length === 1 ? "0" + dateParts[1] : dateParts[1];
                    const day = dateParts[0].length === 1 ? "0" + dateParts[0] : dateParts[0];
                    return dateParts[2] + "-" + month + "-" + day;// + "T00:00Z";
                }
                window.apifyParseDate = parseDate;
                
                const parseDateTime = (date) => {
                    // 5.3.2019 19:13 => 2019-03-05T19:13:00.1+01:00
                    const dateTimeParts = date.split(' ');
                    if (dateTimeParts.length != 2) return null;
                    const dateParts = dateTimeParts[0].split('.');
                    if (dateParts.length != 3) return null;
                    const month = dateParts[1].length === 1 ? "0" + dateParts[1] : dateParts[1];
                    const day = dateParts[0].length === 1 ? "0" + dateParts[0] : dateParts[0];
                    return dateParts[2] + "-" + month + "-" + day + "T" + dateTimeParts[1] + ":00.1+01:00";
                }
                window.apifyParseDateTime = parseDateTime;
            });
            return page.goto(request.url, { timeout: 60000 });
        },
    });

    // Run the crawler and wait for it to finish.
    await crawler.run();

    console.log('Crawler finished.');
});
