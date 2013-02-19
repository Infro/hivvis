using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
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
            string linkageFileName = "Links.txt";
            string articleFileName = "Articles.csv";
            int current = 0;
            //myQuery      = "( \"hiv\"[MeSH Terms] OR \"hiv\"[All Fields]) OR (\"acquired immunodeficiency syndrome\"[MeSH Terms] OR (\"acquired\"[All Fields] AND \"immunodeficiency\"[All Fields] AND \"syndrome\"[All Fields]) OR \"acquired immunodeficiency syndrome\"[All Fields] OR \"aids\"[All Fields])";
            eUtils.eUtilsServiceSoapClient utilServ = new eUtils.eUtilsServiceSoapClient();
            string database = null;
            //database = "PMC";
            eUtils.eSearchResult searchResult = GetSearchResults(database, myQuery, utilServ);

            TextWriter LinksOutputFile = new StreamWriter(linkageFileName);
            TextWriter ArticleOutputFile = new StreamWriter(articleFileName);
            ArticleOutputFile.WriteLine("PMID,Date,Title,Abstract,MeSH,Citation,Grant");

            int Total = int.Parse(searchResult.Count);

            while (searchResult != null)
            {
                foreach (string paper in searchResult.IdList)
                {
                    #region Article Links
                    eUtils.eLinkResult linkResults = GetLinkResults(paper, utilServ);
                    //if (linkResults.LinkSet.Length != 0)
                    //{
                    LinksOutputFile.Write(paper);
                    foreach (var a in linkResults.LinkSet)
                        foreach (var b in a.LinkSetDb)
                            foreach (var c in b.Link)
                                if (c.Id.Value != paper && b.DbTo.ToLower() == "pubmed")
                                    LinksOutputFile.Write(" " + c.Id.Value);
                    LinksOutputFile.WriteLine();
                    //}
                    //else
                    //{
                    //    //No References!?!?
                    //    //Should probably remove from Dataset.
                    //    throw new ArgumentNullException("linkResults", "PubMED ID: " + paper + " has no references, which is improbable to impossible.  Throwing Error");
                    //}
                    #endregion
                    #region Article Title, Abstract, and Date
                    eFetch_pubmed.eUtilsServiceSoapClient fetchServ = new eFetch_pubmed.eUtilsServiceSoapClient();
                    eFetch_pubmed.eFetchRequest fetchRequest = new eFetch_pubmed.eFetchRequest();
                    fetchRequest.id = paper;
                    //fetchRequest.id = string.Join(",", Results);
                    eFetch_pubmed.eFetchResult fetchResult = fetchServ.run_eFetch(fetchRequest);
                    for (int i = 0; i < fetchResult.PubmedArticleSet.Length; i++)
                    {
                        eFetch_pubmed.PubmedArticleType art = null;
                        //eFetch_pubmed.PubmedBookArticleType book = null;

                        if (fetchResult.PubmedArticleSet[i] is eFetch_pubmed.PubmedArticleType)
                        {
                            art = (eFetch_pubmed.PubmedArticleType)fetchResult.PubmedArticleSet[i];
                            ArticleOutputFile.Write(string.Join(",",
                                art.MedlineCitation.PMID.Value,
                                art.PubmedData.History[0].Year + " " + art.PubmedData.History[0].Month + " " + art.PubmedData.History[0].Day,
                                //art.MedlineCitation.Article.ArticleDate != null ? art.MedlineCitation.Article.ArticleDate[0].Year + " " + art.MedlineCitation.Article.ArticleDate[0].Month + " " + art.MedlineCitation.Article.ArticleDate[0].Day : "N/A",
                                '"' + art.MedlineCitation.Article.ArticleTitle.Value.Replace("\"", "''") + '"',
                                '"' + art.MedlineCitation.Article.Abstract.AbstractText[0].Value.Replace("\"", "''") + '"',
                                art.MedlineCitation.MeshHeadingList != null ? '"' + string.Join(" ", art.MedlineCitation.MeshHeadingList.Select(o => o.DescriptorName.Value)) + '"' : "N/A",
                                art.MedlineCitation.CitationSubset != null ? string.Join(" ", art.MedlineCitation.CitationSubset) : "N/A",
                                (art.MedlineCitation.Article.GrantList != null && art.MedlineCitation.Article.GrantList.Grant != null) ?
                                string.Join(" ", art.MedlineCitation.Article.GrantList.Grant.Select(o => o.Agency)) : "N/A"));
                            ArticleOutputFile.WriteLine();
                            //foreach (var v in art.MedlineCitation.Article.Abstract.AbstractText)
                            //Console.WriteLine(v.Value);
                            //foreach(string a in art.MedlineCitation.CitationSubset)
                            //Console.WriteLine(a);
                        }
                        else if (fetchResult.PubmedArticleSet[i] is eFetch_pubmed.PubmedBookArticleType) { }
                        else if (fetchResult.PubmedArticleSet[i] is eFetch_pubmed.PubmedBookDataType) { }
                        else if (fetchResult.PubmedArticleSet[i] is eFetch_pubmed.PubmedDataType) { }
                        else if (fetchResult.PubmedArticleSet[i] is eFetch_pubmed.PubMedPubDateType) { }
                        else if (fetchResult.PubmedArticleSet[i] is eFetch_pubmed.PubMedPubDateTypePubStatus) { }
                    }
                    #endregion
                }
                searchResult = GetSearchResults(database, myQuery, utilServ, searchResult);
            }
            LinksOutputFile.Flush();
            LinksOutputFile.Close();
            LinksOutputFile.Dispose();
            ArticleOutputFile.Flush();
            ArticleOutputFile.Close();
            ArticleOutputFile.Dispose();
        }

        private static eUtils.eLinkResult GetLinkResults(string id, eUtils.eUtilsServiceSoapClient serv)
        {
            eUtils.eLinkRequest req = new eUtils.eLinkRequest();
            req.id = new string[]{id};
            //req.db = "pubmed";
            return serv.run_eLink(req);
        }

        private static eUtils.eSearchResult GetSearchResults(string db, string myQuery, eUtils.eUtilsServiceSoapClient serv, eUtils.eSearchResult searchResult = null)
        {
            eUtils.eSearchRequest req = new eUtils.eSearchRequest();

            //req.db = "pmc";
            req.term = HttpUtility.UrlEncode(myQuery);
            req.db = db;
            req.usehistory = "y";
            if (searchResult != null)
            {
                int current = (int.Parse(searchResult.RetStart) + int.Parse(searchResult.RetMax));
                if (current >= int.Parse(searchResult.Count))
                    return null;
                req.RetStart = current.ToString();
                req.WebEnv = searchResult.WebEnv;
            }

            eUtils.eSearchResult res = serv.run_eSearch(req);
            req.WebEnv = res.WebEnv;
            req.QueryKey = res.QueryKey;
            return res;
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