using System;
using Xunit;

namespace Tests
{
    public class PackerTest
    {
        [Fact]
        public void Test1()
        {
            string actual = "Woof!";

            Assert.Equal("Woof!", actual);
        }
    }
}
