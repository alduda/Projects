using System.IO;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GuaranteedRate;

namespace UnitTestProject
{
    [TestClass]
    public class DelimitedSerializerTest
    {
        [TestMethod]
        public void DeserializeTest()
        {
            string myTestString = "Ann , JonesComma , Female , 200 , 23/6/1967";
            DelimitedSerializer<Person> deserializer = new DelimitedSerializer<Person>();
            IEnumerable<Person> result;
            using (TextReader sr = new StringReader(myTestString))//tests comma separators
            {
                result = deserializer.Deserialize(sr);
                Assert.AreEqual(result.Count(), 1);
            }
            myTestString = "Ann JonesSpace Female 200 23/6/1967";//tests space separators
            using (TextReader sr = new StringReader(myTestString))
            {
                result = deserializer.Deserialize(sr);
                Assert.AreEqual(result.Count(), 1);
            }
            myTestString = "Ann | JonesPipe | Female | 200 | 23/6/1967";//tests pipe separators
            using (TextReader sr = new StringReader(myTestString))
            {
                result = deserializer.Deserialize(sr);
                Assert.AreEqual(result.Count(), 1);
            }
            myTestString = "Ann : JonesColon : Female : 200 : 23/6/1967";//tests against invalid separators
            using (TextReader sr = new StringReader(myTestString))
            {
                result = deserializer.Deserialize(sr);
                Assert.AreEqual(result.Count(), 0);
            }
            myTestString = "Ann | JonesComma | Female | 23/6/1967";//tests expression which is missing entry
            using (TextReader sr = new StringReader(myTestString))
            {
                result = deserializer.Deserialize(sr);
                Assert.AreEqual(result.Count(), 0);
            }
            myTestString = "Ann | JonesComma | Female | 200 | Extra entry | 23/6/1967";//tests expression which extra entry
            using (TextReader sr = new StringReader(myTestString))
            {
                result = deserializer.Deserialize(sr);
                Assert.AreEqual(result.Count(), 0);
            }
            myTestString = "Ann JonesComma Female 200 23/6/1967\nAnn JonesSpace Female 200 23/6/1967\nAnn | JonesPipe | Female | 200 | 23/6/1967";
            //tests 3 different separator in one file
            using (TextReader sr = new StringReader(myTestString))
            {
                result = deserializer.Deserialize(sr);
                Assert.AreEqual(result.Count(), 3);
            }
            myTestString = "Ann JonesComma : Female 200 23/6/1967\nAnn JonesSpace Female 200 23/6/1967\nAnn | JonesPipe | Female | 200 | 23/6/1967";
            //tests 3 different separator in one file with one invalid separator
            using (TextReader sr = new StringReader(myTestString))
            {
                result = deserializer.Deserialize(sr);
                Assert.AreEqual(result.Count(), 2);
            }
            myTestString = "Ann JonesSpace Female 200 23/66/1967";//Invalid date format
            using (TextReader sr = new StringReader(myTestString))
            {
                result = deserializer.Deserialize(sr);
                Assert.AreEqual(result, null);
            }
            myTestString = "Ann JonesSpace Female asdd 23/6/1967";//Invalid color format
            using (TextReader sr = new StringReader(myTestString))
            {
                result = deserializer.Deserialize(sr);
                Assert.AreEqual(result, null);
            }
        }
    }
}
