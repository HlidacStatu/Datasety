# Actor pro extrakci dokumentu z VeKLEPu (Elektronická knihovna legislativního procesu)

Actor stahuje informace o dokumentech z webu https://apps.odok.cz/veklep-vyhledavani
Je dostupny na platforme Apify, lze pouzit z library zde: ToDo

Actor pouziva pro extrakci dat [PuppeteerCrawler](https://sdk.apify.com/docs/api/puppeteercrawler) z [Apify SDK](https://sdk.apify.com/).
Flow vypada nasledovne:
1. Actor otevre https://apps.odok.cz/veklep-vyhledavani
2. klikne na "Hledat", objevi se na privni strance s pagination a dale zpracovava stranky v queue paralelne
3. na strance s pagination prida to queue vsechny stranky s detaily dokumentu (typicky 5) a dalsi stranku v pagination
4. na strance s detailem stahne postupne vsechny informace ze vsech 3 tabu (musi na ne klikat a cekat podle selektoru)
7. kdyz uz neni v queue zadna stranka, skonci

Vysledky se behem crawlovani pridaji do datasetu v Apify a posilaji do Hlidace statu (ten zatim neumi batch import)
