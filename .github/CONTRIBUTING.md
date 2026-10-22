# How to contribute

We welcome community pull requests for bug fixes, enhancements, and documentation. Code and API documentation are in this repo. Conceptual documentation is in the [EntityFramework.Docs](https://github.com/dotnet/EntityFramework.Docs) repo.

For code changes, you will need to [fork and clone the repo and build the code locally](../docs/getting-and-building-the-code.md).

Here is a [video](https://www.youtube.com/watch?v=9OMxy1wal1s) where members of the Entity Framework Core team provide guidance, advice and samples on how to contribute to this project.

## Choosing an issue

All contributions should address an [open issue](https://github.com/dotnet/efcore/issues) in the [dotnet/efcore](https://github.com/dotnet/efcore) repo.

### Bugs versus enhancements

Issues are typically labeled with [type-enhancement](https://github.com/dotnet/efcore/issues?q=is%3Aopen+is%3Aissue+label%3Atype-enhancement) or [type-bug](https://github.com/dotnet/efcore/issues?q=is%3Aopen+is%3Aissue+label%3Atype-bug).

* Bugs are places where EF Core is doing something that it was not designed to.
* Enhancements are suggestions to improve EF Core by changing existing or adding new functionality.

Bugs are usually relatively small, self-contained changes. However, this does not necessarily make them easy to work on, since finding the root cause and fixing it without breaking any other functionality can be tricky.

Enhancements can be anything from tiny changes to support a variation on an existing scenario, to massive cross-cutting features that will take many months to implement. The bigger the enhancement, the more important it is to communicate with the EF Team before working on a contribution.

### Good first issues

Most issues are available to be tackled by the community, even if not explicitly labeled as such. However, two labels are specifically about community contributions:

* Issues that we believe are relatively straightforward to implement are labeled with [good-first-issue](https://github.com/dotnet/efcore/issues?q=is%3Aopen+is%3Aissue+label%3A%22good+first+issue%22). These are good candidates for community pull requests.
* Issues labeled with [help-wanted](https://github.com/dotnet/efcore/issues?q=is%3Aopen+is%3Aissue+label%3A%22help+wanted%22) indicate that the issue requires some specialist expertise to implement. If you have expertise in the relevant area, then working on these issues can be of great help to the EF team. Note that these issues **are not typically easy to implement**.

Issues labeled with [needs-design](https://github.com/dotnet/efcore/issues?q=is%3Aopen+is%3Aissue+label%3Aneeds-design) indicate that the EF Core team has not solidified on an approach to tackle the issue. This typically makes the issue much more difficult for the community to implement.

### Create an issue

If there is no existing issue tracking the change you want to make, then [create one](https://github.com/dotnet/efcore/issues/new/choose)! PRs that don't get merged are often those that are created without any prior discussion with the team. An issue is the best place to have that discussion, ideally before the PR is submitted.

### Fixing typos

An issue is not required for simple non-code changes like fixing a typo in documentation. In fact, these changes can often be submitted as a PR directly from the browser, avoiding the need to fork and clone.

## Workflow

The typical workflow for contributing to EF Core is outlined below. This is not a set-in-stone process, but rather guidelines to help ensure a quality PR that we can merge efficiently.

* [Set up your development environment](../docs/getting-and-building-the-code.md) so that you can build and test the code. Don't forget to [create a fork](https://docs.github.com/en/github/getting-started-with-github/fork-a-repo) for your work.
* Make sure all tests are passing. (This is typically done by running `test` at a command prompt.)
* Choose an issue (see above), understand it, and **comment on the issue** indicating what you intend to do to fix it. **This communication with the team is very important and often helps avoid throwing away lots of work caused by taking the wrong approach.**
* Create and check out a [branch](https://docs.github.com/en/github/collaborating-with-issues-and-pull-requests/creating-and-deleting-branches-within-your-repository) in your local clone. You will use this branch to prepare your PR.
* Make appropriate code and test changes. Follow the patterns and code style that you see in the existing code. Make sure to add tests that fail without the change and then pass with the change.
* Consider other scenarios where your change may have an impact and add more testing. We always prefer having too many tests to having not enough of them.
* When you are done with changes, make sure _all_ existing tests are still passing. (Again, typically by running `test` at a command prompt.)
  * EF Core tests contain many "SQL assertions" - these verify that the precise expected SQL is generated for all scenarios. Some changes can cause SQL alterations to ripple across many scenarios, and changing all expected SQL in assertions is quite laborious. If you set the `EF_TEST_REWRITE_BASELINES` environment variable to `1` and then run the tests, the SQL assertions in the source code will be automatically changed to contain the new SQL baselines.
* Commit changes to your branch and push the branch to your GitHub fork.
* Go to the main [EF Core repo](https://github.com/dotnet/efcore/pulls) and you should see a yellow box suggesting you create a PR from your fork. Do this, or [create the PR by some other mechanism](https://docs.github.com/en/github/collaborating-with-issues-and-pull-requests/about-pull-requests).
* Sign the [contributor license agreement](https://cla.dotnetfoundation.org/) if you have not already done so.
* Wait for the feedback from the team and for the continuous integration (C.I.) checks to pass.
* Add and push new commits to your branch to address any issues.

The PR will be merged by a member of the EF Team once the C.I. checks have passed and the code has been approved.

## Developer builds

By default, the EF build requires API documentation for all public members. However, this can be turned off while actively working on code by creating a file named "AspNetCoreSettings.props" above the repo root (for example, in the folder that itself contains your solution folder) with the following contents:

```xml
<Project>
  <PropertyGroup>
    <DeveloperBuild>True</DeveloperBuild>
  </PropertyGroup>
</Project>
```

## Breaking changes

EF Core is used by many thousands of existing applications. We want to make it as easy as possible for those existing applications to update to new versions. A change that causes an existing application to break when being updated is known as a "breaking change". Sometimes it is necessary to make a breaking change to keep the platform alive and moving forward. However, each such breaking change must be explicitly called out and will only be approved if the value of making the change greatly outweighs the pain of breaking existing applications.

### API breaking changes

The easiest type of breaking change to identify are those that change the public API surface such that existing code no longer works. API breaks come in two forms:

* Source breaking changes happen when existing code fails to compile against the new public API surface.
* Binary breaking changes happen when an assembly compiled against an older version of EF Core fails to work when run with a new version of EF Core.

For example, consider the following class:

```C#
public class Foo
{
    public Foo(string a)
    {
        A = a;
    }

    public string A { get; }
}
```
Assuming we shipped this, and now want to add a new parameter to the constructor:

```C#
public class Foo
{
    public Foo(string a, string b)
    {
        A = a;
        B = b;
    }

    public string A { get; }
    public string B { get; }
}
```

This is both a source breaking change and a binary breaking change. The single parameter constructor is gone, so existing assemblies cannot bind to it, and existing source code cannot compile against it.

What if instead we made the new parameter optional?

```C#
public class Foo
{
    public Foo(string a, string b = null)
    {
        A = a;
        B = b;
    }

    public string A { get; }
    public string B { get; }
}
```

Now this is a binary breaking change, but _not_ a source breaking change. Source that previously compiled against the single parameter constructor can still compile against the new two-parameter constructor. However, assemblies attempting to bind to the single parameter constructor will still not find it.

Binary breaking changes, especially for database providers, can have lower impact and may be acceptable. However, where possible try to write code that is not breaking at all. For example, in this case by adding a new constructor overload:

```C#
public class Foo
{
    public Foo(string a)
        : this(a, null)
    {
    }

    public Foo(string a, string b)
    {
        A = a;
        B = b;
    }

    public string A { get; }
    public string B { get; }
}
```

This is now neither a source nor a binary breaking change.

### Behavioral breaking changes

Behavioral breaking changes happen when the behavior of calling some API changes without the API itself changing. For example, if calling `DbContext.Add` previously resulted in an entity in the `Added` state, and now results in it being in the `Modified` state, then that's a behavioral break. Existing applications will not be expecting a `Modified` entity and may therefore not behave correctly.

Behavioral breaking changes are hard to identify and are one of the reasons testing is so important. Running all the existing tests can help find behavioral breaks. Likewise, implementing good tests with your change helps ensure any future breaks will be discovered.

## Code-of-conduct

Please remember to abide by our [code-of-conduct](../.github/CODE_OF_CONDUCT.md) while working on contributions.
