using System;
using DD4T.Templates.Base.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DD4T.Templates.Test
{
    [TestClass]
    public class MD5Hash
    {
        [TestMethod]
        public void TestMD5Hash()
        {
            var source = "tcm:1109-29182-4_customer-detail-carousel-automfg-534x308_2357561535297132.jpg";
            var hash = BinaryPublisher.MD5Hash(source);
            Assert.AreEqual("0A9C432007DB54E3ED3F940E7D4573B7", hash);
        }
    }
}
