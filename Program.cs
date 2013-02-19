using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Download_PubMED.eUtils;
using System.IO;

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
            myQuery = "(\"hiv\"[MeSH Terms] OR \"hiv\"[All Fields]) OR (\"acquired immunodeficiency syndrome\"[MeSH Terms] OR (\"acquired\"[All Fields] AND \"immunodeficiency\"[All Fields] AND \"syndrome\"[All Fields]) OR \"acquired immunodeficiency syndrome\"[All Fields] OR \"aids\"[All Fields])";
            eUtils.eUtilsServiceSoapClient serv = new eUtils.eUtilsServiceSoapClient();
            string database = null;
            database = "PMC";
            eUtils.eSearchResult searchResult = GetSearchResults(database, myQuery, serv);
            List<string> Results = new List<string>(searchResult.IdList);
            TextWriter LinksOutputFile = new StreamWriter("Links.txt");
            //List<string> Links = new List<string>();
            foreach (string paper in Results)
            //string paper = Results[0];
            {
                eUtils.eLinkResult linkResults = GetLinkResults(paper, serv);
                if (linkResults.LinkSet.Length != 0)
                {
                    foreach (var a in linkResults.LinkSet)
                        foreach (var b in a.LinkSetDb)
                            foreach (var c in b.Link)
                                if(c.Id.Value != paper)
                                    LinksOutputFile.WriteLine(string.Format("pubmed,{0} {2},{1}", paper, c.Id.Value, b.DbTo));
                                    //Links.Add(string.Format("pubmed,{0} {2},{1}", paper, c.Id.Value, b.DbTo));
                    //linkResults.LinkSet.SelectMany<LinkSetType, string>(o => o.LinkSetDb.SelectMany<LinkSetDbType,string>(o.IdList.Select<IdType, string>(v => v.Value)));

                    //Add Paper to Paper Network
                    //If no references inside pubmed, it will throw an error
                    //eUtils.LinkSetType ls = linkResults.LinkSet.First(o => o.DbFrom.ToLower() == "pubmed");
                    //linkResults.LinkSet.SelectMany(o => o.LinkSetDb
                }
                else
                {
                    //No References!?!?
                    //Should probably remove from Dataset.
                    throw new ArgumentNullException("linkResults", "PubMED ID: " + paper + " has no references, which is improbable to impossible.  Throwing Error");
                }
            }
            LinksOutputFile.Flush();
            LinksOutputFile.Close();
            LinksOutputFile.Dispose();
        }

        private static eUtils.eLinkResult GetLinkResults(string id, eUtils.eUtilsServiceSoapClient serv)
        {
            eUtils.eLinkRequest req = new eUtils.eLinkRequest();
            req.id = new string[]{id};
            //req.db = "pubmed";
            return serv.run_eLink(req);
        }

        private static eUtils.eSearchResult GetSearchResults(string db, string myQuery, eUtils.eUtilsServiceSoapClient serv)
        {
            eUtils.eSearchRequest req = new eUtils.eSearchRequest();

            //req.db = "pmc";
            req.term = HttpUtility.UrlEncode(myQuery);
            req.db = db;
            req.usehistory = "y";

            eUtils.eSearchResult res = serv.run_eSearch(req);
            req.WebEnv = res.WebEnv;
            req.QueryKey = res.QueryKey;
            return res;
        }

        private static IEnumerable<eSearchResult> GetAllSearchResults(string myQuery, eUtils.eUtilsServiceSoapClient serv)
        {
            throw new NotImplementedException();
            return null;
        }
    }
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