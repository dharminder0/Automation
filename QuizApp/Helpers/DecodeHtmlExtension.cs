using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Helpers {
	public static class StringExtension {
        public static string DecodeHtml(string Text) {
            try {
                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(Text);
                string decodedString = htmlDocument.DocumentNode.InnerText;
                return decodedString;
            } catch (Exception ex) {

                return Text;
            }

        }
    }
}