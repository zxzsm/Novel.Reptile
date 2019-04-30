using AngleSharp.Parser.Html;
using Novel.Reptile.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Novel.Reptile.BookLink
{
    /// <summary>
    /// 笔趣云
    /// biquyu.com
    /// </summary>
    public class BQYLink : LinkBase
    {
        public BQYLink(BookLinks bookLinks) : base(bookLinks)
        {
        }

        public override void ReadLink()
        {
            using (BookContext bookContext = new BookContext())
            {
                var html = HttpHelper.HttpGet(Links.Url);
                var parser = new HtmlParser();
                var document = parser.Parse(html);
                var metas = document.QuerySelectorAll("meta");
                Task(bookContext, metas, syncType: 2);
                bookContext.BookLinks.Remove(Links);
                bookContext.SaveChanges();
            }
        }

        
    }
}
