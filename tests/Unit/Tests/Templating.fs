namespace Bolero.Tests.Web

open System
open System.Globalization
open NUnit.Framework
open OpenQA.Selenium
open OpenQA.Selenium.Support.UI

/// HTML Templates.
[<Category "Templating">]
module Templating =

    let elt = NodeFixture(By.Id "test-fixture-templating")

    let blur() =
        WebFixture.Driver
            .ExecuteScript("document.activeElement.blur()")
            |> ignore

    [<Test>]
    let ``Inline template is instantiated``() =
        Assert.IsNotNull(elt.ByClass("inline"))

    [<Test>]
    let ``File template is instantiated``() =
        Assert.IsNotNull(elt.ByClass("file"))

    [<Test>]
    let ``Node hole filled with string``() =
        Assert.AreEqual("NodeHole1 content",
            elt.ByClass("nodehole1").Text)

    [<Test>]
    let ``File template node hole filled``() =
        Assert.IsNotNull(elt.ByClass("file").ByClass("file-hole"))

    [<Test>]
    let ``Node hole filled with node``() =
        let filledWith = elt.ByClass("nodehole2-content")
        Assert.IsNotNull(filledWith)
        Assert.AreEqual("NodeHole2 content", filledWith.Text)

    [<Test>]
    [<TestCase("nodehole3-1")>]
    [<TestCase("nodehole3-2")>]
    let ``Node hole filled with string [multiple]``(id: string) =
        Assert.AreEqual("NodeHole3 content", elt.ByClass(id).Text)

    [<Test>]
    [<TestCase("nodehole4-1")>]
    [<TestCase("nodehole4-2")>]
    let ``Node hole filled with node [multiple]``(id: string) =
        let elt = elt.ByClass(id)
        let filledWith = elt.ByClass("nodehole4-content")
        Assert.IsNotNull(filledWith)
        Assert.AreEqual("NodeHole4 content", filledWith.Text)

    [<Test>]
    let ``Attr hole``() =
        Assert.Contains("attrhole1-content",
            elt.ByClass("attrhole1").GetAttribute("class").Split(' '))

    [<Test>]
    [<TestCase("attrhole2-1")>]
    [<TestCase("attrhole2-2")>]
    let ``Attr hole [multiple]``(id: string) =
        Assert.Contains("attrhole2-content",
            elt.ByClass(id).GetAttribute("class").Split(' '))

    [<Test>]
    let ``Attr hole mixed with node hole``() =
        Assert.Contains("attrhole3-content",
            elt.ByClass("attrhole3-1").GetAttribute("class").Split(' '))
        Assert.AreEqual("attrhole3-content",
            elt.ByClass("attrhole3-2").Text)

    [<Test>]
    let ``Full attr hole``() =
        let elt = elt.ByClass("fullattrhole")
        Assert.AreEqual("fullattrhole-content", elt.GetAttribute("id"))
        Assert.AreEqual("1234", elt.GetAttribute("data-fullattrhole"))

    [<Test>]
    let ``Attr hole obj value``() =
        let elt = elt.ByClass("attrhole4")
        Assert.AreEqual("5678", elt.GetAttribute("data-value"))
        Assert.IsNotNull(elt.GetAttribute("data-true"))
        Assert.IsNull(elt.GetAttribute("data-false"))

    [<Test>]
    let ``Event hole``() =
        let elt = elt.Inner(By.ClassName "events")
        let state = elt.ByClass("currentstate")
        let position = elt.ByClass("position")
        let isNumber (s: string) =
            Double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, ref 0.)
        let isValidPosition() =
            let a = position.Text.Split(',')
            isNumber a.[0] && isNumber a.[1]

        elt.ByClass("btn1").Click()
        elt.AssertAreEqualEventually("clicked 1",
            (fun () -> state.Text),
            "First click")
        Assert.IsTrue(isValidPosition(), "Position: " + position.Text)

        elt.ByClass("btn2").Click()
        elt.AssertAreEqualEventually("clicked 2",
            (fun () -> state.Text),
            "Second click")
        Assert.IsTrue(isValidPosition(), "Position: " + position.Text)

        elt.ByClass("btn3").Click()
        elt.AssertAreEqualEventually("clicked 1",
            (fun () -> state.Text),
            "Same event bound multiple times")
        Assert.IsTrue(isValidPosition(), "Position: " + position.Text)

    [<Test>]
    [<TestCase("")>]
    [<TestCase("-onchange")>]
    let ``Bind string to normal input``(cls: string) =
        let elt = elt.Inner(By.ClassName "binds")
        let inp = elt.ByClass(sprintf "input%s1-1" cls)
        inp.Clear()
        inp.SendKeys("hello")
        if cls.Contains("onchange") then blur()
        elt.AssertAreEqualEventually("hello",
            (fun () -> elt.ByClass(sprintf "display%s1" cls).Text),
            "Value propagation")
        Assert.AreEqual("hello",
            elt.ByClass(sprintf "input%s1-2" cls).GetAttribute("value"),
            "Propagation to other input")
        Assert.AreEqual("hello",
            elt.ByClass(sprintf "textarea%s1" cls).GetAttribute("value"),
            "Propagation to textarea")
        Assert.AreEqual("hello",
            elt.ByClass(sprintf "select%s1" cls).GetAttribute("value"),
            "Propagation to select")

    [<Test>]
    [<TestCase("")>]
    [<TestCase("-onchange")>]
    let ``Bind string to textarea``(cls: string) =
        let elt = elt.Inner(By.ClassName "binds")
        let inp = elt.ByClass(sprintf"textarea%s1" cls)
        inp.Clear()
        inp.SendKeys("hi textarea")
        if cls.Contains("onchange") then blur()
        elt.AssertAreEqualEventually("hi textarea",
            (fun () -> elt.ByClass(sprintf "display%s1" cls).Text),
            "Value propagation")
        Assert.AreEqual("hi textarea",
            elt.ByClass(sprintf "input%s1-1" cls).GetAttribute("value"),
            "Propagation to input")
        Assert.AreEqual("hi textarea",
            elt.ByClass(sprintf "input%s1-2" cls).GetAttribute("value"),
            "Propagation to other input")
        Assert.AreEqual("hi textarea",
            elt.ByClass(sprintf "select%s1" cls).GetAttribute("value"),
            "Propagation to select")

    [<Test>]
    let ``Bind string to select``() =
        let elt = elt.Inner(By.ClassName "binds")
        SelectElement(elt.ByClass("select1"))
            .SelectByValue("hi select")
        elt.AssertAreEqualEventually("hi select",
            (fun () -> elt.ByClass("display1").Text),
            "Value propagation")
        Assert.AreEqual("hi select",
            elt.ByClass("input1-1").GetAttribute("value"),
            "Propagation to input")
        Assert.AreEqual("hi select",
            elt.ByClass("input1-2").GetAttribute("value"),
            "Propagation to other input")
        Assert.AreEqual("hi select",
            elt.ByClass("textarea1").GetAttribute("value"),
            "Propagation to textarea")

    [<Test>]
    [<TestCase("")>]
    [<TestCase("-onchange")>]
    let ``Bind int``(cls: string) =
        let elt = elt.Inner(By.ClassName "binds")
        let inp = elt.ByClass(sprintf "input%s2-1" cls)
        inp.Clear()
        inp.SendKeys("1234")
        if cls.Contains("onchange") then blur()
        elt.AssertAreEqualEventually("1234",
            (fun () -> elt.ByClass(sprintf "display%s2" cls).Text),
            "Value propagation")
        Assert.AreEqual("1234",
            elt.ByClass(sprintf "input%s2-2" cls).GetAttribute("value"),
            "Propagation to other input")

    [<Test>]
    [<TestCase("", Ignore = "Char-by-char parsing may eat the dot, TODO fix")>]
    [<TestCase("-onchange")>]
    let ``Bind float``(cls: string) =
        let elt = elt.Inner(By.ClassName "binds")
        let inp = elt.ByClass(sprintf "input%s3-1" cls)
        inp.Clear()
        inp.SendKeys("123.456")
        if cls.Contains("onchange") then blur()
        elt.AssertAreEqualEventually("123.456",
            (fun () -> elt.ByClass(sprintf "display%s3" cls).Text),
            "Value propagation")
        Assert.AreEqual("123.456",
            elt.ByClass(sprintf "input%s3-2" cls).GetAttribute("value"),
            "Propagation to other input")

    [<Test>]
    let ``Bind checkbox``() =
        let elt = elt.Inner(By.ClassName "binds")
        let inp1 = elt.ByClass("input4-1")
        let inp2 = elt.ByClass("input4-2")
        let isChecked (inp: IWebElement) =
            match inp.GetAttribute("checked") with
            | null -> false
            | s -> bool.Parse s
        let initial = false
        Assert.AreEqual(initial, isChecked inp1)
        Assert.AreEqual(initial, isChecked inp2)
        inp1.Click()
        elt.AssertAreEqualEventually(not initial,
            (fun () -> isChecked inp1),
            "Click inp1 toggles checked1")
        elt.AssertAreEqualEventually(not initial,
            (fun () -> isChecked inp2),
            "Click inp1 toggles checked2")
        inp2.Click()
        elt.AssertAreEqualEventually(initial,
            (fun () -> isChecked inp1),
            "Click inp2 toggles checked1")
        elt.AssertAreEqualEventually(initial,
            (fun () -> isChecked inp2),
            "Click inp2 toggles checked2")

    [<Test>]
    let ``Nested template is instantiated``() =
        Assert.IsNotNull(elt.ByClass("nested1"))

    [<Test>]
    let ``Nested template is removed from its original parent``() =
        Assert.IsNull(elt.ById("Nested1"))

    [<Test>]
    let ``Nested template hole filled``() =
        Assert.IsNotNull(elt.ByClass("nested1").ByClass("nested-hole"))
        Assert.IsNull(elt.ByClass("nested1").ByClass("file-hole"))

    [<Test>]
    let ``Recursively nested template is instantiated``() =
        Assert.IsNotNull(elt.ByClass("nested2"))

    [<Test>]
    let ``Recursively nested template is removed from its original parent``() =
        Assert.IsNull(elt.ById("Nested2"))

    [<Test>]
    let ``Regression #11: common hole in attrs and children``() =
        Assert.AreEqual("regression-11", elt.ByClass("regression-11").Text)
