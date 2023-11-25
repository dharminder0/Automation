using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuizApp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Tests {
    [TestClass]
    public class CommunicationHelperTests {

        [TestMethod]
        public void SendMailWithAttachment_Test() {
            //Arrange
            var files = new List<Models.FileAttachment> {
                new Models.FileAttachment {
                    FileLink = "https://jrcontent.blob.core.windows.net/emailattachmentdev/02758803-d554-fileexamplepdf500kb.pdf",
                    FileName = "02758803-d554-fileexamplepdf500kb.pdf"
                }
            };
            //Act
            var result = CommunicationHelper.SendMailWithAttachment("mostafa.elmoghazi@jobrock.com", "Test from appointment", "This is the test", "AT", files);
            //Assert
            result.Should().BeTrue();
        }
    }
}