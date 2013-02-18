using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Download_PubMED
{
    class Program
    {
        //EInfo, Get Databases
        //EGQuery, Search all databases and return database/count
        //ESearch, Search a database and return a list of UIDs
        //ESummary, Turn UID into Document Summary
        //EFetch, Turn UID into formatted data records
        //EFetch, see http://www.ncbi.nlm.nih.gov/books/NBK25499/table/chapter4.chapter4_table1/?report=objectonly
        //ELink, List links, and more
        //ELink, see http://www.ncbi.nlm.nih.gov/books/NBK25499/#chapter4.ELink
        //EPost, Append UIDs to a list of UIDs
        //ESpell, Suggests ?improved queries? and spelling fixes.

        static void Main(string[] args)
        {
            string myQuery = "((\"hiv\"[MeSH Terms] OR \"hiv\"[All Fields]) OR (\"acquired immunodeficiency syndrome\"[MeSH Terms] OR (\"acquired\"[All Fields] AND \"immunodeficiency\"[All Fields] AND \"syndrome\"[All Fields]) OR \"acquired immunodeficiency syndrome\"[All Fields] OR \"aids\"[All Fields])) AND hasabstract[text]";
            string Result;
            //try
            //{
                Result = string.Empty;
                eUtils.eUtilsServiceSoapClient serv = new eUtils.eUtilsServiceSoapClient();
                ////////////////////////////////////////////////////////////////////////////////////////

                //eUtils.eSearchRequest req = new eUtils.eSearchRequest()
                //{
                //    db = "pmc",
                //    sort = "SortDate",
                //    term = "hiv",
                //    RetStart = "0",
                //    RetMax = "15"
                //};
                //eUtils.eSearchResult res = serv.run_eSearch(req);
            
            // NOTE: search term should be URL encoded

                eUtils.eSearchRequest req = new eUtils.eSearchRequest();

                //req.db = "pmc";
                req.term = HttpUtility.UrlEncode(myQuery);
                req.usehistory = "y";

                eUtils.eSearchResult res = serv.run_eSearch(req);
                req.WebEnv = res.WebEnv;
                req.QueryKey = res.QueryKey;
                for (int i = 0; i < res.IdList.Length; i++)
                {
                    Result += res.IdList[i] + "\n";
                }

                //string theQuery = myQuery;
                //eUtils.eGqueryRequest theRequest = new eUtils.eGqueryRequest()
                //    {
                //        term = theQuery
                //    };
                //eUtils.Result theResult = serv.run_eGquery(theRequest);

                //for (int i = 0; i < theResult.eGQueryResult.ResultItem.Length; i++)
                //{

                //    Result +=
                //        "  " + theResult.eGQueryResult.ResultItem[i].DbName +
                //        ": " + theResult.eGQueryResult.ResultItem[i].Count + "\r\n";

                //}
                //eUtils.eInfoResult res = serv.run_eInfo(new eUtils.eInfoRequest());
                //for (int i = 0; i < res.DbList.Items.Length; i++)
                //{

                //    Result += res.DbList.Items[i] + "\r\n";
                //}
            //}

            //catch (Exception eee)
            //{

            //    Result = eee.ToString();

            //}
            Console.WriteLine(Result);
            Console.ReadKey();
        }
    }
}
