using System;
using System.Linq;
using ShadowWriter;

namespace ShadowWriter.Tests;

[TestFixture]
[TestOf(typeof(BuildEmbeddedResourceOutputModel))]
public class BuildEmbeddedResourceOutputModelTests
{
    [Test]
    public void EmptyList()
    {
       var sut = new  BuildEmbeddedResourceOutputModel("a");

       var result = sut.GeneratedClasses([]);

       result.InnerClasses.ShouldBeEmpty();
    }

    [Test]
    public void SingleItem()
    {
        var sut = new  BuildEmbeddedResourceOutputModel("a");

        var result = sut.GeneratedClasses(["res/Image1.jpg"]);

        result.InnerClasses.Count.ShouldBe(1);
        result.InnerClasses[0].Items.Count.ShouldBe(1);

        var item  =result.InnerClasses[0].Items[0];

        item.PropertyName.ShouldBe("Image1Jpg");
        item.ManifestResourceName.ShouldBe("a.res.Image1.jpg");
    }

    [Test]
    public void SingleItemInRootFolder()
    {
        var sut = new  BuildEmbeddedResourceOutputModel("a");

        var result = sut.GeneratedClasses(["Image1.jpg"]);

        result.InnerClasses.ShouldBeEmpty();
        result.Items.Count.ShouldBe(1);
    }

    [Test]
    public void TwoItemsInSameFolder()
    {
        var sut = new  BuildEmbeddedResourceOutputModel("a");

        var result = sut.GeneratedClasses(["res/Image1.jpg", "res/Image2.jpg"]);

        result.InnerClasses.Count.ShouldBe(1);
        result.InnerClasses[0].Items.Count.ShouldBe(2);
        result.InnerClasses[0].Name.ShouldBe("res");
    }

    [Test]
    public void MultipleItemsInDifferentFolders()
    {
        var sut = new  BuildEmbeddedResourceOutputModel("a");

        var result = sut.GeneratedClasses(["res1/Image1.jpg", "res2/Image2.jpg", "res1/Image3.jpg", "res1/a/ImageX.jpg"]);

        result.InnerClasses.Count.ShouldBe(2);
        result.InnerClasses[0].InnerClasses.Count.ShouldBe(1);
        result.InnerClasses[1].InnerClasses.Count.ShouldBe(0);

    }
}