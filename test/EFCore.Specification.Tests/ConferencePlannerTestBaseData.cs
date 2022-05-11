// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public partial class ConferencePlannerTestBase<TFixture>
{
    public const string ConferenceData = @"[
  {
    ""date"": ""2019-06-19T00:00:00"",
    ""rooms"": [
      {
        ""id"": 4479,
        ""name"": ""Room 1"",
        ""sessions"": [
          {
            ""id"": ""98819"",
            ""title"": ""An introduction to Machine Learning using LEGO"",
            ""description"": ""Where do you start when you want to learn Machine Learning? Do you start by learning some advanced algorithms or perhaps a Machine Learning framework? In this talk, I will show an alternative approach on how to get started with Machine Learning by using a LEGO car as a model. By using Machine Learning, we can make the LEGO car steer autonomously!\r\n\r\nIn the process of making a LEGO car steer autonomously I will go through the following topics:\r\n- What is Machine Learning?\r\n- Can we make the LEGO car steer autonomously without Machine Learning?\r\n- How does the basic theory behind Machine Learning work?\r\n- How do we connect theory to LEGO bricks?\r\n\r\nThe aim of this talk is not to make you an expert in Machine Learning, however, it will hopefully inspire you to investigate and get familiar with Machine Learning in a playful and simple way."",
            ""startsAt"": ""2019-06-19T10:20:00"",
            ""endsAt"": ""2019-06-19T11:20:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""7e84a6a9-f685-4cfa-a31d-9c00ce592b79"",
                ""name"": ""Jeppe Tornfeldt Sørensen""
              }
            ],
            ""categories"": [],
            ""roomId"": 4479,
            ""room"": ""Room 1""
          },
          {
            ""id"": ""99028"",
            ""title"": ""How to Steal an Election"",
            ""description"": ""In this session I'll demonstrate some of the data science techniques that can be used to influence a population to a particular way of thinking, or to vote in a particular way. With all the talk of collusion in the American, and other, elections I think it's about time we explore the art of the possible in this field, and see just how they might have done it."",
            ""startsAt"": ""2019-06-19T11:40:00"",
            ""endsAt"": ""2019-06-19T12:40:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""2a5c6285-1012-49e7-a0f6-1efdeea7dba9"",
                ""name"": ""Gary Short""
              }
            ],
            ""categories"": [],
            ""roomId"": 4479,
            ""room"": ""Room 1""
          },
          {
            ""id"": ""84070"",
            ""title"": ""Why databases cry at night?"",
            ""description"": ""In the dark of the night, if you listen carefully enough, you can hear databases cry. But why? As developers, we rarely consider what happens under the hood of widely used abstractions such as databases. As a consequence, we rarely think about the performance of databases. This is especially true to less widespread, but often very useful NoSQL databases.\r\n\r\nIn this talk we will take a close look at NoSQL database performance, peek under the hood of the most frequently used features to see how they affect performance and discuss performance issues and bottlenecks inherent to all databases."",
            ""startsAt"": ""2019-06-19T13:40:00"",
            ""endsAt"": ""2019-06-19T14:40:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""f70aa2e2-a961-4c7f-bb77-e5ab82692ee9"",
                ""name"": ""Michael Yarichuk""
              }
            ],
            ""categories"": [],
            ""roomId"": 4479,
            ""room"": ""Room 1""
          },
          {
            ""id"": ""99466"",
            ""title"": ""Drones & AI - What's all the buzz about ?"",
            ""description"": ""Drones and AI are changing our world.\r\n\r\nIn this session we will look at some of the real world solutions utilising these emerging technologies, you will get an understanding of the core use cases, learn how to get started with the tech, and find out about the pitfalls to avoid when building solutions with drones and Artificial Intelligence."",
            ""startsAt"": ""2019-06-19T15:00:00"",
            ""endsAt"": ""2019-06-19T16:00:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""81ccb948-0157-4ef0-bd80-c11ad108d7cb"",
                ""name"": ""Adam Stephensen""
              }
            ],
            ""categories"": [],
            ""roomId"": 4479,
            ""room"": ""Room 1""
          },
          {
            ""id"": ""98996"",
            ""title"": ""Automatic text summarization"",
            ""description"": ""Automatic text summarization is the process of shortening a text document by automatically creating a short, accurate, and fluent summary with the main points of the original document using software. It is a common problem in machine learning and natural language processing.  \r\n\r\nSince humans have the capacity to understand the meaning of a text document and extract the most important information from the original source using their own words, we are generally quite good at making summaries of a text. However, manual creation of summaries is very time consuming, and therefore a need for automatic summary has arisen. Not only are the automatic summarization tools much faster, they are also less biased than humans. \r\n\r\nNowadays, there are several methods of text summary, but there are two basic approaches to text summary that are based on the output type: extractive and abstractive. In an extractive summary, the most important sentences are extracted and joined to get a brief summary. The abstract text summary algorithms create new sentences and sentences that provide the most useful information from the original text - just as humans do. \r\n\r\nThis lecture provides insight into most common algorithms and tools used for automatic text summarization today, together with the methods used to evaluate automated summaries. \r\n"",
            ""startsAt"": ""2019-06-19T16:20:00"",
            ""endsAt"": ""2019-06-19T17:20:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""e47fa275-9f67-4ef4-8571-28d1b17d667a"",
                ""name"": ""Masa Nekic""
              }
            ],
            ""categories"": [],
            ""roomId"": 4479,
            ""room"": ""Room 1""
          },
          {
            ""id"": ""97550"",
            ""title"": ""Entity Framework debugging using SQL Server: A Detective Story"",
            ""description"": ""What happens when the code for your Entity Framework Core LINQ queries looks good, but your app is very slow? Are you looking in the right place? Don’t be afraid to start looking at your database. Knowing how to investigate and debug what your LINQ queries are doing in SQL Server is as important as the actual LINQ query in your .NET solutions. We will be looking at database server configurations, using MSSQL database profiling tools and understanding Query Execution Plans to get the most out of Entity Framework. In the end, learning to be an Entity Framework detective will make your project sound and snappy."",
            ""startsAt"": ""2019-06-19T17:40:00"",
            ""endsAt"": ""2019-06-19T18:40:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""215bbd42-ce3a-4744-af1f-7e5f0f30d620"",
                ""name"": ""Chris Woodruff""
              }
            ],
            ""categories"": [],
            ""roomId"": 4479,
            ""room"": ""Room 1""
          }
        ],
        ""hasOnlyPlenumSessions"": false
      },
      {
        ""id"": 4480,
        ""name"": ""Room 2"",
        ""sessions"": [
          {
            ""id"": ""97632"",
            ""title"": ""C# and Rust: combining managed and unmanaged code without sacrificing safety"",
            ""description"": ""Why would you ever want to introduce unmanaged code into your managed codebase when recent versions of C# have made writing high performance code in .NET more accessible than ever before? While C# has been pushing downwards into the realm of \""systems programming\"", Rust, a language that already operates in this space, has been pushing upwards. The result isn't a competition where one language must emerge as more universally applicable than the other. It's a broader set of cases where C# and Rust can work together, and integrating them effectively is more idiomatic thanks to this broadening of scope.\r\n\r\nWhen we set out in 2018 to rebuild the storage engine for our log server, Seq, we decided to complement our existing C# codebase with a new one written in Rust. In this talk we'll look at why you might want to add unmanaged code to your managed codebase, using Seq as an example. We'll explore how to use the tools that .NET and Rust give us to design and build a safe and robust foreign function interface. In the end we'll have a new perspective on the implicit safety contracts we're expected to uphold in our purely managed codebases."",
            ""startsAt"": ""2019-06-19T10:20:00"",
            ""endsAt"": ""2019-06-19T11:20:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""0029ba03-068c-44eb-bd23-3cdd17fc725a"",
                ""name"": ""Ashley Mannix""
              }
            ],
            ""categories"": [],
            ""roomId"": 4480,
            ""room"": ""Room 2""
          },
          {
            ""id"": ""119373"",
            ""title"": ""Hidden gems in .NET Core 3"",
            ""description"": ""You've likely heard about the headline features in .NET Core 3.0 including Blazor, gRPC, and Windows desktop app support, but what else is there?\r\n\r\nThis is a big release so come and see David Fowler and Damian Edwards from the .NET Core team showcase their favorite new features you probably haven't heard about in this demo-packed session."",
            ""startsAt"": ""2019-06-19T11:40:00"",
            ""endsAt"": ""2019-06-19T12:40:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""a2c9a4b0-cb47-414a-a7fb-93d0928e77d2"",
                ""name"": ""Damian Edwards""
              },
              {
                ""id"": ""b2959d46-2ae9-494b-865c-fb850e37d24a"",
                ""name"": ""David Fowler""
              }
            ],
            ""categories"": [],
            ""roomId"": 4480,
            ""room"": ""Room 2""
          },
          {
            ""id"": ""81621"",
            ""title"": ""C++: λ Demystified"",
            ""description"": ""C++11 lambdas brought us lambdas. They open a lot of new possibilities. Frequently asked questions are:\r\n- How are they implemented?\r\n- How do they work?\r\n- How can it affect my daily programming?\r\n- Do they generate a lot of code?\r\n- What can I do with them?\r\n- Where can I use them?\r\n\r\nIn this talk I will answer these questions. With the support of C++ Insights (https://cppinsights.io) we will peak behind the scenes to answer questions about how they are implemented. We will also see how the compiler generated code changes, if we change the lambda itself. This is often interesting for development in constrained applications like the embedded domain.\r\n\r\nNext I will show you application areas of lambdas to illustrate where they can be helpful. For example, how they can help you to write less and with that more unique code.\r\n\r\nAfter the talk, you will have a solid understanding of lambdas in C++. Together with some ideas when and where to use them."",
            ""startsAt"": ""2019-06-19T13:40:00"",
            ""endsAt"": ""2019-06-19T14:40:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""0871f66f-f085-4b96-a311-bd7570460627"",
                ""name"": ""Andreas Fertig""
              }
            ],
            ""categories"": [],
            ""roomId"": 4480,
            ""room"": ""Room 2""
          },
          {
            ""id"": ""78447"",
            ""title"": ""Rust for C++ developers - What you need to know to get rolling with crates"",
            ""description"": ""The session is about using the Rust language to write safe, concurrent and elegant code, contrasting with C++"",
            ""startsAt"": ""2019-06-19T15:00:00"",
            ""endsAt"": ""2019-06-19T16:00:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""3b2b49cf-5746-484c-a85e-be960fe76043"",
                ""name"": ""Pavel Yosifovich""
              }
            ],
            ""categories"": [],
            ""roomId"": 4480,
            ""room"": ""Room 2""
          },
          {
            ""id"": ""98569"",
            ""title"": ""Securing Web APIs from JavaScript/SPA Applications"",
            ""description"": ""Modern web development means that more and more application code is running in the browser as JavaScript. This architectural shift requires us to change how we perform authentication and authorization. Fortunately, using modern protocols such as OpenID Connect you don’t need to invent your own solution for this new environment. This session will show you the modern approach for browser-based JavaScript applications to authenticate users, and perform secure web api invocations. As you might expect, security is sufficiently complex and so even modern security comes with its own set of challenges. Luckily, we will show off some libraries that help manage this complexity so your application doesn’t have to."",
            ""startsAt"": ""2019-06-19T16:20:00"",
            ""endsAt"": ""2019-06-19T17:20:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""d438622e-8053-4a8f-8df0-af7e9ad32db0"",
                ""name"": ""Brock Allen""
              }
            ],
            ""categories"": [],
            ""roomId"": 4480,
            ""room"": ""Room 2""
          },
          {
            ""id"": ""98020"",
            ""title"": ""Mechanical C++ Refactoring in the Present and in the Future"",
            ""description"": ""In the last few years, Clang has opened up new possibilities in C++ tooling for the masses. Tools such as clang-tidy offer ready-to-use source-to-source transformations. Available transformations can be used to modernize (use newer C++ language features), improve readability (remove redundant constructs), or improve adherence to the C++ Core Guidelines.\r\n\r\nHowever, when special needs arise, maintainers of large codebases need to learn some of the Clang APIs to create their own porting aids. The Clang APIs necessarily form a more-exact picture of the structure of C++ code than most developers keep in their heads, and bridging the conceptual gap can be a daunting task.\r\n\r\nTooling supplied with clang-tidy, such as clang-query, are indispensable in the discovery of the Clang AST.\r\n\r\nThis talk will show recent and future features in Clang tooling, as well as Tips, Tricks and Traps encountered on the journey to quality refactoring tools. The audience will see how mechanical refactoring in a large codebase can become easy, given the right tools.\r\n"",
            ""startsAt"": ""2019-06-19T17:40:00"",
            ""endsAt"": ""2019-06-19T18:40:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""dae0a4a0-9346-4031-82b0-365633cdd776"",
                ""name"": ""Stephen Kelly""
              }
            ],
            ""categories"": [],
            ""roomId"": 4480,
            ""room"": ""Room 2""
          }
        ],
        ""hasOnlyPlenumSessions"": false
      },
      {
        ""id"": 4481,
        ""name"": ""Room 3"",
        ""sessions"": [
          {
            ""id"": ""77259"",
            ""title"": ""I'm Going To Make You Stop Hating CSS."",
            ""description"": ""As a formalized language, CSS is over 20 years old and has spent much of that time being maligned by the people who use it. Browser inconsistencies, changing specifications and general weirdness have combined to create this weird pseudo-language that you'd rather avoid.\r\n\r\nUNTIL TODAY. With modern specs and tooling, CSS has never been more straightforward and less reliant on hacks.  In this talk, Lemon will show you some common traps people fall in, as well as some general strategies for making a layout grid you can proud to build and confident in releasing."",
            ""startsAt"": ""2019-06-19T10:20:00"",
            ""endsAt"": ""2019-06-19T11:20:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""f4f1eb31-8574-4e8a-a94a-ee0aef8c28d5"",
                ""name"": ""Lemon 🍋""
              }
            ],
            ""categories"": [],
            ""roomId"": 4481,
            ""room"": ""Room 3""
          },
          {
            ""id"": ""97811"",
            ""title"": ""Web components and micro apps, the web technologies peacekeeper"",
            ""description"": ""Web development can be a bit tiring with all these new innovative technologies and frameworks we get on a regular basis especially if you have to maintain a product for a long time.\r\n\r\nHowever, web components and micro apps has given us a solution which is framework independent. It allows for decomposition of a big project into smaller, more maintainable parts that can be handled by different teams with different technologies.\r\n\r\nLet's go on a journey to find out how we can unite best technologies to form our enterprise apps."",
            ""startsAt"": ""2019-06-19T11:40:00"",
            ""endsAt"": ""2019-06-19T12:40:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""a127e924-82ac-42df-8761-ecb106c8ef61"",
                ""name"": ""Yaser Adel Mehraban""
              }
            ],
            ""categories"": [],
            ""roomId"": 4481,
            ""room"": ""Room 3""
          },
          {
            ""id"": ""83799"",
            ""title"": ""Responsible JavaScript"",
            ""description"": ""While the performance of JavaScript engines in browsers have seen continued improvement, the amount of JavaScript we serve increases unabated. We need to use JavaScript more responsibly, which means we must rely on native browser features where prudent, use HTML and CSS when appropriate, and know when too much JavaScript is just that: Too much. \r\n\r\nIn this talk, we'll explore what happens to performance and accessibility when devices are inundated with more JavaScript than they can handle. We'll also dive into some novel techniques you can use to tailor delivery of scripts with respect to a person's device capabilities and network connection quality. When you walk out of this session, you'll be equipped with new knowledge to make your sites as fast as they are beautiful."",
            ""startsAt"": ""2019-06-19T13:40:00"",
            ""endsAt"": ""2019-06-19T14:40:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""c3017677-3f20-4b72-8d3f-a2dfa18f4e99"",
                ""name"": ""Jeremy Wagner""
              }
            ],
            ""categories"": [],
            ""roomId"": 4481,
            ""room"": ""Room 3""
          },
          {
            ""id"": ""97507"",
            ""title"": ""CSS Grid - What is this Magic?!"",
            ""description"": ""We’ve all heard a lot in the last year about a new advancement in the world of CSS, called CSS Grid. Starting off at whispers, we’re now starting to hear it as a deafening roar as more and more developers write about it, talk about it, share it and start using it. In the world of front end, I see it everywhere I turn and am excited as I start to use it in my own projects.\r\n\r\nBut what does this new CSS specification mean for software developers, and why should you care about it? In the world of tech today, we can do so many amazing things and use whatever language we choose across a wide range of devices and platforms. Whether it’s the advent of React and React Native, or frameworks like Electron, it’s easier than ever to build one app that works on multiple platforms with the language we know and work with best. The ability to do this also expands to styling apps on any platform using CSS, and therefore being able to utilise the magical thing that is\r\nCSS Grid.\r\n\r\nThe reason CSS Grid is gaining so much attention, is because it’s a game changer for front end and layouts. With a few simple lines of code, we can now create imaginative, dynamic, responsive layouts (yep, I know that’s a lot of buzz words). While a lot of people are calling this the new ‘table layout’, grid gives us so much more, with the ability to spread cells across columns and rows to whatever size you choose, dictate which direction new items flow, allow cells to move around to fit in place and even tell certain cells exactly where they need to sit.\r\n\r\nWhile there is so much to worry about when developing an app, CSS Grid means that you can worry less about building the layout on the front end, and more about making sure the back end works well. Let me show you how the magic works."",
            ""startsAt"": ""2019-06-19T15:00:00"",
            ""endsAt"": ""2019-06-19T16:00:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""d387e75c-ed26-4dc6-8612-0f18abdfd9f5"",
                ""name"": ""Amy Kapernick""
              }
            ],
            ""categories"": [],
            ""roomId"": 4481,
            ""room"": ""Room 3""
          },
          {
            ""id"": ""98750"",
            ""title"": ""Panel discussion on the future of .NET"",
            ""description"": ""Join us for a discussion with four leaders in the field on the current state of the art and the where .NET and related technologies are heading.\r\n\r\nWe will discuss cross platform development, new features, performance improvements, .NET Core and EF Core 3, what’s going to happen with full framework, Blazor, how .NET stands up against competing technologies and where it is all going.\r\n\r\nYou won't cram more info into a session than this, come spend a great hour with us."",
            ""startsAt"": ""2019-06-19T16:20:00"",
            ""endsAt"": ""2019-06-19T17:20:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""de972e57-7765-4c38-9dcd-5981587c1433"",
                ""name"": ""Bryan Hogan""
              },
              {
                ""id"": ""c9c8096e-47a1-41e5-a00c-d49b51d01c4e"",
                ""name"": ""K. Scott Allen""
              },
              {
                ""id"": ""282a4701-f128-4b1d-bd67-da5d1f8b3eb0"",
                ""name"": ""Julie Lerman""
              },
              {
                ""id"": ""b2959d46-2ae9-494b-865c-fb850e37d24a"",
                ""name"": ""David Fowler""
              },
              {
                ""id"": ""a2c9a4b0-cb47-414a-a7fb-93d0928e77d2"",
                ""name"": ""Damian Edwards""
              }
            ],
            ""categories"": [],
            ""roomId"": 4481,
            ""room"": ""Room 3""
          },
          {
            ""id"": ""98457"",
            ""title"": ""Testing GraphQL: From Zero To Hundred Percent"",
            ""description"": ""Testing is important for every project, whether it's a web application or api service. But writing scripts to test your application can be a hassle, especially for specific frameworks or tools like GraphQL. Sure, you could just test using Jest, Enzyme or any other testing tool out there for JavaScript applications. But how do you specifically test your GraphQL schemas and queries?"",
            ""startsAt"": ""2019-06-19T17:40:00"",
            ""endsAt"": ""2019-06-19T18:40:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""4bb972aa-3d92-45ff-95b4-68ed3ca86e9e"",
                ""name"": ""Roy Derks""
              }
            ],
            ""categories"": [],
            ""roomId"": 4481,
            ""room"": ""Room 3""
          }
        ],
        ""hasOnlyPlenumSessions"": false
      },
      {
        ""id"": 4489,
        ""name"": ""Expo"",
        ""sessions"": [
          {
            ""id"": ""85411"",
            ""title"": ""Keynote: Leadership Guide for the Reluctant Leader"",
            ""description"": ""Regardless of the technology you know, regardless of the job title you have, you have amazing potential to impact your workplace, community, and beyond.\r\n\r\nIn this talk, I’ll share a few candid stories of my career failures… I mean… learning opportunities. We’ll start by debunking the myth that leadership == management. Next, we’ll talk about some the attributes, behaviors and skills of good leaders. Last, we’ll cover some practical steps and resources to accelerate your journey.\r\n\r\nYou’ll walk away with some essential leadership skills I believe anyone can develop, and a good dose of encouragement to be more awesome!"",
            ""startsAt"": ""2019-06-19T09:00:00"",
            ""endsAt"": ""2019-06-19T10:00:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""3884cc4d-8364-4316-9b9a-e16561d87af3"",
                ""name"": ""David Neal""
              }
            ],
            ""categories"": [],
            ""roomId"": 4489,
            ""room"": ""Expo""
          }
        ],
        ""hasOnlyPlenumSessions"": false
      }
    ],
    ""timeSlots"": [
      {
        ""slotStart"": ""09:00:00"",
        ""rooms"": [
          {
            ""id"": 4489,
            ""name"": ""Expo"",
            ""session"": {
              ""id"": ""85411"",
              ""title"": ""Keynote: Leadership Guide for the Reluctant Leader"",
              ""description"": ""Regardless of the technology you know, regardless of the job title you have, you have amazing potential to impact your workplace, community, and beyond.\r\n\r\nIn this talk, I’ll share a few candid stories of my career failures… I mean… learning opportunities. We’ll start by debunking the myth that leadership == management. Next, we’ll talk about some the attributes, behaviors and skills of good leaders. Last, we’ll cover some practical steps and resources to accelerate your journey.\r\n\r\nYou’ll walk away with some essential leadership skills I believe anyone can develop, and a good dose of encouragement to be more awesome!"",
              ""startsAt"": ""2019-06-19T09:00:00"",
              ""endsAt"": ""2019-06-19T10:00:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""3884cc4d-8364-4316-9b9a-e16561d87af3"",
                  ""name"": ""David Neal""
                }
              ],
              ""categories"": [],
              ""roomId"": 4489,
              ""room"": ""Expo""
            },
            ""index"": 11
          }
        ]
      },
      {
        ""slotStart"": ""10:20:00"",
        ""rooms"": [
          {
            ""id"": 4479,
            ""name"": ""Room 1"",
            ""session"": {
              ""id"": ""98819"",
              ""title"": ""An introduction to Machine Learning using LEGO"",
              ""description"": ""Where do you start when you want to learn Machine Learning? Do you start by learning some advanced algorithms or perhaps a Machine Learning framework? In this talk, I will show an alternative approach on how to get started with Machine Learning by using a LEGO car as a model. By using Machine Learning, we can make the LEGO car steer autonomously!\r\n\r\nIn the process of making a LEGO car steer autonomously I will go through the following topics:\r\n- What is Machine Learning?\r\n- Can we make the LEGO car steer autonomously without Machine Learning?\r\n- How does the basic theory behind Machine Learning work?\r\n- How do we connect theory to LEGO bricks?\r\n\r\nThe aim of this talk is not to make you an expert in Machine Learning, however, it will hopefully inspire you to investigate and get familiar with Machine Learning in a playful and simple way."",
              ""startsAt"": ""2019-06-19T10:20:00"",
              ""endsAt"": ""2019-06-19T11:20:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""7e84a6a9-f685-4cfa-a31d-9c00ce592b79"",
                  ""name"": ""Jeppe Tornfeldt Sørensen""
                }
              ],
              ""categories"": [],
              ""roomId"": 4479,
              ""room"": ""Room 1""
            },
            ""index"": 0
          },
          {
            ""id"": 4480,
            ""name"": ""Room 2"",
            ""session"": {
              ""id"": ""97632"",
              ""title"": ""C# and Rust: combining managed and unmanaged code without sacrificing safety"",
              ""description"": ""Why would you ever want to introduce unmanaged code into your managed codebase when recent versions of C# have made writing high performance code in .NET more accessible than ever before? While C# has been pushing downwards into the realm of \""systems programming\"", Rust, a language that already operates in this space, has been pushing upwards. The result isn't a competition where one language must emerge as more universally applicable than the other. It's a broader set of cases where C# and Rust can work together, and integrating them effectively is more idiomatic thanks to this broadening of scope.\r\n\r\nWhen we set out in 2018 to rebuild the storage engine for our log server, Seq, we decided to complement our existing C# codebase with a new one written in Rust. In this talk we'll look at why you might want to add unmanaged code to your managed codebase, using Seq as an example. We'll explore how to use the tools that .NET and Rust give us to design and build a safe and robust foreign function interface. In the end we'll have a new perspective on the implicit safety contracts we're expected to uphold in our purely managed codebases."",
              ""startsAt"": ""2019-06-19T10:20:00"",
              ""endsAt"": ""2019-06-19T11:20:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""0029ba03-068c-44eb-bd23-3cdd17fc725a"",
                  ""name"": ""Ashley Mannix""
                }
              ],
              ""categories"": [],
              ""roomId"": 4480,
              ""room"": ""Room 2""
            },
            ""index"": 1
          },
          {
            ""id"": 4481,
            ""name"": ""Room 3"",
            ""session"": {
              ""id"": ""77259"",
              ""title"": ""I'm Going To Make You Stop Hating CSS."",
              ""description"": ""As a formalized language, CSS is over 20 years old and has spent much of that time being maligned by the people who use it. Browser inconsistencies, changing specifications and general weirdness have combined to create this weird pseudo-language that you'd rather avoid.\r\n\r\nUNTIL TODAY. With modern specs and tooling, CSS has never been more straightforward and less reliant on hacks.  In this talk, Lemon will show you some common traps people fall in, as well as some general strategies for making a layout grid you can proud to build and confident in releasing."",
              ""startsAt"": ""2019-06-19T10:20:00"",
              ""endsAt"": ""2019-06-19T11:20:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""f4f1eb31-8574-4e8a-a94a-ee0aef8c28d5"",
                  ""name"": ""Lemon 🍋""
                }
              ],
              ""categories"": [],
              ""roomId"": 4481,
              ""room"": ""Room 3""
            },
            ""index"": 2
          }
        ]
      },
      {
        ""slotStart"": ""11:40:00"",
        ""rooms"": [
          {
            ""id"": 4479,
            ""name"": ""Room 1"",
            ""session"": {
              ""id"": ""99028"",
              ""title"": ""How to Steal an Election"",
              ""description"": ""In this session I'll demonstrate some of the data science techniques that can be used to influence a population to a particular way of thinking, or to vote in a particular way. With all the talk of collusion in the American, and other, elections I think it's about time we explore the art of the possible in this field, and see just how they might have done it."",
              ""startsAt"": ""2019-06-19T11:40:00"",
              ""endsAt"": ""2019-06-19T12:40:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""2a5c6285-1012-49e7-a0f6-1efdeea7dba9"",
                  ""name"": ""Gary Short""
                }
              ],
              ""categories"": [],
              ""roomId"": 4479,
              ""room"": ""Room 1""
            },
            ""index"": 0
          },
          {
            ""id"": 4480,
            ""name"": ""Room 2"",
            ""session"": {
              ""id"": ""119373"",
              ""title"": ""Hidden gems in .NET Core 3"",
              ""description"": ""You've likely heard about the headline features in .NET Core 3.0 including Blazor, gRPC, and Windows desktop app support, but what else is there?\r\n\r\nThis is a big release so come and see David Fowler and Damian Edwards from the .NET Core team showcase their favorite new features you probably haven't heard about in this demo-packed session."",
              ""startsAt"": ""2019-06-19T11:40:00"",
              ""endsAt"": ""2019-06-19T12:40:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""a2c9a4b0-cb47-414a-a7fb-93d0928e77d2"",
                  ""name"": ""Damian Edwards""
                },
                {
                  ""id"": ""b2959d46-2ae9-494b-865c-fb850e37d24a"",
                  ""name"": ""David Fowler""
                }
              ],
              ""categories"": [],
              ""roomId"": 4480,
              ""room"": ""Room 2""
            },
            ""index"": 1
          },
          {
            ""id"": 4481,
            ""name"": ""Room 3"",
            ""session"": {
              ""id"": ""97811"",
              ""title"": ""Web components and micro apps, the web technologies peacekeeper"",
              ""description"": ""Web development can be a bit tiring with all these new innovative technologies and frameworks we get on a regular basis especially if you have to maintain a product for a long time.\r\n\r\nHowever, web components and micro apps has given us a solution which is framework independent. It allows for decomposition of a big project into smaller, more maintainable parts that can be handled by different teams with different technologies.\r\n\r\nLet's go on a journey to find out how we can unite best technologies to form our enterprise apps."",
              ""startsAt"": ""2019-06-19T11:40:00"",
              ""endsAt"": ""2019-06-19T12:40:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""a127e924-82ac-42df-8761-ecb106c8ef61"",
                  ""name"": ""Yaser Adel Mehraban""
                }
              ],
              ""categories"": [],
              ""roomId"": 4481,
              ""room"": ""Room 3""
            },
            ""index"": 2
          }
        ]
      },
      {
        ""slotStart"": ""13:40:00"",
        ""rooms"": [
          {
            ""id"": 4479,
            ""name"": ""Room 1"",
            ""session"": {
              ""id"": ""84070"",
              ""title"": ""Why databases cry at night?"",
              ""description"": ""In the dark of the night, if you listen carefully enough, you can hear databases cry. But why? As developers, we rarely consider what happens under the hood of widely used abstractions such as databases. As a consequence, we rarely think about the performance of databases. This is especially true to less widespread, but often very useful NoSQL databases.\r\n\r\nIn this talk we will take a close look at NoSQL database performance, peek under the hood of the most frequently used features to see how they affect performance and discuss performance issues and bottlenecks inherent to all databases."",
              ""startsAt"": ""2019-06-19T13:40:00"",
              ""endsAt"": ""2019-06-19T14:40:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""f70aa2e2-a961-4c7f-bb77-e5ab82692ee9"",
                  ""name"": ""Michael Yarichuk""
                }
              ],
              ""categories"": [],
              ""roomId"": 4479,
              ""room"": ""Room 1""
            },
            ""index"": 0
          },
          {
            ""id"": 4480,
            ""name"": ""Room 2"",
            ""session"": {
              ""id"": ""81621"",
              ""title"": ""C++: λ Demystified"",
              ""description"": ""C++11 lambdas brought us lambdas. They open a lot of new possibilities. Frequently asked questions are:\r\n- How are they implemented?\r\n- How do they work?\r\n- How can it affect my daily programming?\r\n- Do they generate a lot of code?\r\n- What can I do with them?\r\n- Where can I use them?\r\n\r\nIn this talk I will answer these questions. With the support of C++ Insights (https://cppinsights.io) we will peak behind the scenes to answer questions about how they are implemented. We will also see how the compiler generated code changes, if we change the lambda itself. This is often interesting for development in constrained applications like the embedded domain.\r\n\r\nNext I will show you application areas of lambdas to illustrate where they can be helpful. For example, how they can help you to write less and with that more unique code.\r\n\r\nAfter the talk, you will have a solid understanding of lambdas in C++. Together with some ideas when and where to use them."",
              ""startsAt"": ""2019-06-19T13:40:00"",
              ""endsAt"": ""2019-06-19T14:40:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""0871f66f-f085-4b96-a311-bd7570460627"",
                  ""name"": ""Andreas Fertig""
                }
              ],
              ""categories"": [],
              ""roomId"": 4480,
              ""room"": ""Room 2""
            },
            ""index"": 1
          },
          {
            ""id"": 4481,
            ""name"": ""Room 3"",
            ""session"": {
              ""id"": ""83799"",
              ""title"": ""Responsible JavaScript"",
              ""description"": ""While the performance of JavaScript engines in browsers have seen continued improvement, the amount of JavaScript we serve increases unabated. We need to use JavaScript more responsibly, which means we must rely on native browser features where prudent, use HTML and CSS when appropriate, and know when too much JavaScript is just that: Too much. \r\n\r\nIn this talk, we'll explore what happens to performance and accessibility when devices are inundated with more JavaScript than they can handle. We'll also dive into some novel techniques you can use to tailor delivery of scripts with respect to a person's device capabilities and network connection quality. When you walk out of this session, you'll be equipped with new knowledge to make your sites as fast as they are beautiful."",
              ""startsAt"": ""2019-06-19T13:40:00"",
              ""endsAt"": ""2019-06-19T14:40:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""c3017677-3f20-4b72-8d3f-a2dfa18f4e99"",
                  ""name"": ""Jeremy Wagner""
                }
              ],
              ""categories"": [],
              ""roomId"": 4481,
              ""room"": ""Room 3""
            },
            ""index"": 2
          }
        ]
      },
      {
        ""slotStart"": ""15:00:00"",
        ""rooms"": [
          {
            ""id"": 4479,
            ""name"": ""Room 1"",
            ""session"": {
              ""id"": ""99466"",
              ""title"": ""Drones & AI - What's all the buzz about ?"",
              ""description"": ""Drones and AI are changing our world.\r\n\r\nIn this session we will look at some of the real world solutions utilising these emerging technologies, you will get an understanding of the core use cases, learn how to get started with the tech, and find out about the pitfalls to avoid when building solutions with drones and Artificial Intelligence."",
              ""startsAt"": ""2019-06-19T15:00:00"",
              ""endsAt"": ""2019-06-19T16:00:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""81ccb948-0157-4ef0-bd80-c11ad108d7cb"",
                  ""name"": ""Adam Stephensen""
                }
              ],
              ""categories"": [],
              ""roomId"": 4479,
              ""room"": ""Room 1""
            },
            ""index"": 0
          },
          {
            ""id"": 4480,
            ""name"": ""Room 2"",
            ""session"": {
              ""id"": ""78447"",
              ""title"": ""Rust for C++ developers - What you need to know to get rolling with crates"",
              ""description"": ""The session is about using the Rust language to write safe, concurrent and elegant code, contrasting with C++"",
              ""startsAt"": ""2019-06-19T15:00:00"",
              ""endsAt"": ""2019-06-19T16:00:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""3b2b49cf-5746-484c-a85e-be960fe76043"",
                  ""name"": ""Pavel Yosifovich""
                }
              ],
              ""categories"": [],
              ""roomId"": 4480,
              ""room"": ""Room 2""
            },
            ""index"": 1
          },
          {
            ""id"": 4481,
            ""name"": ""Room 3"",
            ""session"": {
              ""id"": ""97507"",
              ""title"": ""CSS Grid - What is this Magic?!"",
              ""description"": ""We’ve all heard a lot in the last year about a new advancement in the world of CSS, called CSS Grid. Starting off at whispers, we’re now starting to hear it as a deafening roar as more and more developers write about it, talk about it, share it and start using it. In the world of front end, I see it everywhere I turn and am excited as I start to use it in my own projects.\r\n\r\nBut what does this new CSS specification mean for software developers, and why should you care about it? In the world of tech today, we can do so many amazing things and use whatever language we choose across a wide range of devices and platforms. Whether it’s the advent of React and React Native, or frameworks like Electron, it’s easier than ever to build one app that works on multiple platforms with the language we know and work with best. The ability to do this also expands to styling apps on any platform using CSS, and therefore being able to utilise the magical thing that is\r\nCSS Grid.\r\n\r\nThe reason CSS Grid is gaining so much attention, is because it’s a game changer for front end and layouts. With a few simple lines of code, we can now create imaginative, dynamic, responsive layouts (yep, I know that’s a lot of buzz words). While a lot of people are calling this the new ‘table layout’, grid gives us so much more, with the ability to spread cells across columns and rows to whatever size you choose, dictate which direction new items flow, allow cells to move around to fit in place and even tell certain cells exactly where they need to sit.\r\n\r\nWhile there is so much to worry about when developing an app, CSS Grid means that you can worry less about building the layout on the front end, and more about making sure the back end works well. Let me show you how the magic works."",
              ""startsAt"": ""2019-06-19T15:00:00"",
              ""endsAt"": ""2019-06-19T16:00:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""d387e75c-ed26-4dc6-8612-0f18abdfd9f5"",
                  ""name"": ""Amy Kapernick""
                }
              ],
              ""categories"": [],
              ""roomId"": 4481,
              ""room"": ""Room 3""
            },
            ""index"": 2
          }
        ]
      },
      {
        ""slotStart"": ""16:20:00"",
        ""rooms"": [
          {
            ""id"": 4479,
            ""name"": ""Room 1"",
            ""session"": {
              ""id"": ""98996"",
              ""title"": ""Automatic text summarization"",
              ""description"": ""Automatic text summarization is the process of shortening a text document by automatically creating a short, accurate, and fluent summary with the main points of the original document using software. It is a common problem in machine learning and natural language processing.  \r\n\r\nSince humans have the capacity to understand the meaning of a text document and extract the most important information from the original source using their own words, we are generally quite good at making summaries of a text. However, manual creation of summaries is very time consuming, and therefore a need for automatic summary has arisen. Not only are the automatic summarization tools much faster, they are also less biased than humans. \r\n\r\nNowadays, there are several methods of text summary, but there are two basic approaches to text summary that are based on the output type: extractive and abstractive. In an extractive summary, the most important sentences are extracted and joined to get a brief summary. The abstract text summary algorithms create new sentences and sentences that provide the most useful information from the original text - just as humans do. \r\n\r\nThis lecture provides insight into most common algorithms and tools used for automatic text summarization today, together with the methods used to evaluate automated summaries. \r\n"",
              ""startsAt"": ""2019-06-19T16:20:00"",
              ""endsAt"": ""2019-06-19T17:20:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""e47fa275-9f67-4ef4-8571-28d1b17d667a"",
                  ""name"": ""Masa Nekic""
                }
              ],
              ""categories"": [],
              ""roomId"": 4479,
              ""room"": ""Room 1""
            },
            ""index"": 0
          },
          {
            ""id"": 4480,
            ""name"": ""Room 2"",
            ""session"": {
              ""id"": ""98569"",
              ""title"": ""Securing Web APIs from JavaScript/SPA Applications"",
              ""description"": ""Modern web development means that more and more application code is running in the browser as JavaScript. This architectural shift requires us to change how we perform authentication and authorization. Fortunately, using modern protocols such as OpenID Connect you don’t need to invent your own solution for this new environment. This session will show you the modern approach for browser-based JavaScript applications to authenticate users, and perform secure web api invocations. As you might expect, security is sufficiently complex and so even modern security comes with its own set of challenges. Luckily, we will show off some libraries that help manage this complexity so your application doesn’t have to."",
              ""startsAt"": ""2019-06-19T16:20:00"",
              ""endsAt"": ""2019-06-19T17:20:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""d438622e-8053-4a8f-8df0-af7e9ad32db0"",
                  ""name"": ""Brock Allen""
                }
              ],
              ""categories"": [],
              ""roomId"": 4480,
              ""room"": ""Room 2""
            },
            ""index"": 1
          },
          {
            ""id"": 4481,
            ""name"": ""Room 3"",
            ""session"": {
              ""id"": ""98750"",
              ""title"": ""Panel discussion on the future of .NET"",
              ""description"": ""Join us for a discussion with four leaders in the field on the current state of the art and the where .NET and related technologies are heading.\r\n\r\nWe will discuss cross platform development, new features, performance improvements, .NET Core and EF Core 3, what’s going to happen with full framework, Blazor, how .NET stands up against competing technologies and where it is all going.\r\n\r\nYou won't cram more info into a session than this, come spend a great hour with us."",
              ""startsAt"": ""2019-06-19T16:20:00"",
              ""endsAt"": ""2019-06-19T17:20:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""de972e57-7765-4c38-9dcd-5981587c1433"",
                  ""name"": ""Bryan Hogan""
                },
                {
                  ""id"": ""c9c8096e-47a1-41e5-a00c-d49b51d01c4e"",
                  ""name"": ""K. Scott Allen""
                },
                {
                  ""id"": ""282a4701-f128-4b1d-bd67-da5d1f8b3eb0"",
                  ""name"": ""Julie Lerman""
                },
                {
                  ""id"": ""b2959d46-2ae9-494b-865c-fb850e37d24a"",
                  ""name"": ""David Fowler""
                },
                {
                  ""id"": ""a2c9a4b0-cb47-414a-a7fb-93d0928e77d2"",
                  ""name"": ""Damian Edwards""
                }
              ],
              ""categories"": [],
              ""roomId"": 4481,
              ""room"": ""Room 3""
            },
            ""index"": 2
          }
        ]
      },
      {
        ""slotStart"": ""17:40:00"",
        ""rooms"": [
          {
            ""id"": 4479,
            ""name"": ""Room 1"",
            ""session"": {
              ""id"": ""97550"",
              ""title"": ""Entity Framework debugging using SQL Server: A Detective Story"",
              ""description"": ""What happens when the code for your Entity Framework Core LINQ queries looks good, but your app is very slow? Are you looking in the right place? Don’t be afraid to start looking at your database. Knowing how to investigate and debug what your LINQ queries are doing in SQL Server is as important as the actual LINQ query in your .NET solutions. We will be looking at database server configurations, using MSSQL database profiling tools and understanding Query Execution Plans to get the most out of Entity Framework. In the end, learning to be an Entity Framework detective will make your project sound and snappy."",
              ""startsAt"": ""2019-06-19T17:40:00"",
              ""endsAt"": ""2019-06-19T18:40:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""215bbd42-ce3a-4744-af1f-7e5f0f30d620"",
                  ""name"": ""Chris Woodruff""
                }
              ],
              ""categories"": [],
              ""roomId"": 4479,
              ""room"": ""Room 1""
            },
            ""index"": 0
          },
          {
            ""id"": 4480,
            ""name"": ""Room 2"",
            ""session"": {
              ""id"": ""98020"",
              ""title"": ""Mechanical C++ Refactoring in the Present and in the Future"",
              ""description"": ""In the last few years, Clang has opened up new possibilities in C++ tooling for the masses. Tools such as clang-tidy offer ready-to-use source-to-source transformations. Available transformations can be used to modernize (use newer C++ language features), improve readability (remove redundant constructs), or improve adherence to the C++ Core Guidelines.\r\n\r\nHowever, when special needs arise, maintainers of large codebases need to learn some of the Clang APIs to create their own porting aids. The Clang APIs necessarily form a more-exact picture of the structure of C++ code than most developers keep in their heads, and bridging the conceptual gap can be a daunting task.\r\n\r\nTooling supplied with clang-tidy, such as clang-query, are indispensable in the discovery of the Clang AST.\r\n\r\nThis talk will show recent and future features in Clang tooling, as well as Tips, Tricks and Traps encountered on the journey to quality refactoring tools. The audience will see how mechanical refactoring in a large codebase can become easy, given the right tools.\r\n"",
              ""startsAt"": ""2019-06-19T17:40:00"",
              ""endsAt"": ""2019-06-19T18:40:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""dae0a4a0-9346-4031-82b0-365633cdd776"",
                  ""name"": ""Stephen Kelly""
                }
              ],
              ""categories"": [],
              ""roomId"": 4480,
              ""room"": ""Room 2""
            },
            ""index"": 1
          },
          {
            ""id"": 4481,
            ""name"": ""Room 3"",
            ""session"": {
              ""id"": ""98457"",
              ""title"": ""Testing GraphQL: From Zero To Hundred Percent"",
              ""description"": ""Testing is important for every project, whether it's a web application or api service. But writing scripts to test your application can be a hassle, especially for specific frameworks or tools like GraphQL. Sure, you could just test using Jest, Enzyme or any other testing tool out there for JavaScript applications. But how do you specifically test your GraphQL schemas and queries?"",
              ""startsAt"": ""2019-06-19T17:40:00"",
              ""endsAt"": ""2019-06-19T18:40:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""4bb972aa-3d92-45ff-95b4-68ed3ca86e9e"",
                  ""name"": ""Roy Derks""
                }
              ],
              ""categories"": [],
              ""roomId"": 4481,
              ""room"": ""Room 3""
            },
            ""index"": 2
          }
        ]
      }
    ]
  },
  {
    ""date"": ""2019-06-20T00:00:00"",
    ""rooms"": [
      {
        ""id"": 4479,
        ""name"": ""Room 1"",
        ""sessions"": [
          {
            ""id"": ""74808"",
            ""title"": ""Getting Started with Cosmos DB + EF Core"",
            ""description"": ""Cosmos DB is great and awesomely fast. Wouldn't be even more amazing if we could use our beloved entity framework to manage it? Let see how we can wire it up and get started"",
            ""startsAt"": ""2019-06-20T09:00:00"",
            ""endsAt"": ""2019-06-20T10:00:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""c739e2f1-ecf5-43e1-abcf-90cf13dd7b8f"",
                ""name"": ""Thiago Passos""
              }
            ],
            ""categories"": [],
            ""roomId"": 4479,
            ""room"": ""Room 1""
          },
          {
            ""id"": ""98994"",
            ""title"": ""Deep Learning in the world of little ponies"",
            ""description"": ""In this talk, we will discuss computer vision, one of the most common real-world applications of machine learning. We will deep dive into various state-of-the-art concepts around building custom image classifiers - application of deep neural networks, specifically convolutional neural networks and transfer learning. We will demonstrate how those approaches could be used to create your own image classifier to recognise the characters of \""My Little Pony\"" TV Series [or Pokemon, or Superheroes, or your custom images]."",
            ""startsAt"": ""2019-06-20T10:20:00"",
            ""endsAt"": ""2019-06-20T11:20:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""b4c506ac-9757-4ac6-8b7c-3d6696b113e4"",
                ""name"": ""Galiya Warrier""
              }
            ],
            ""categories"": [],
            ""roomId"": 4479,
            ""room"": ""Room 1""
          },
          {
            ""id"": ""80032"",
            ""title"": ""Living in eventually consistent reality"",
            ""description"": ""Strongly consistent databases are dominating world of software. However, with increasing scale and global availability of our services, many developers often prefer to loose their constraints in favor of an eventual consistency. \r\n\r\nDuring this presentation we'll talk about Conflict-free Replicated Data Types (CRDT) - an eventually-consistent structures, that can be found in many modern day multi-master, geo-distributed databases such as CosmosDB, DynamoDB, Riak, Cassandra or Redis: how do they work and what makes them so interesting choice in highly available systems."",
            ""startsAt"": ""2019-06-20T11:40:00"",
            ""endsAt"": ""2019-06-20T12:40:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""6373f8a6-11a6-43e9-b1ee-a898dd02d1ce"",
                ""name"": ""Bartosz Sypytkowski""
              }
            ],
            ""categories"": [],
            ""roomId"": 4479,
            ""room"": ""Room 1""
          },
          {
            ""id"": ""98933"",
            ""title"": ""Is AI right for me?"",
            ""description"": ""Artificial intelligence (AI) has become a form of Swiss Army knife for the enterprise world. If you have a data problem, throw some AI at it! However, this mentality can lead to wasted time and money going down the path of implementing a heavy-handed solution that doesn’t fit your business problem. Navigating the waters of AI, machine learning and data analysis can be tricky, especially when being sold by the myriad of data science and AI companies offering solutions at the enterprise level. But fear not, some simple guidelines can help. In this talk, I will present a basic rubric for evaluating AI and data analytics techniques as potential solutions for enterprise business problems."",
            ""startsAt"": ""2019-06-20T13:40:00"",
            ""endsAt"": ""2019-06-20T14:40:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""e1864dca-8a88-4ea7-840a-20b88702b38f"",
                ""name"": ""Amber McKenzie""
              }
            ],
            ""categories"": [],
            ""roomId"": 4479,
            ""room"": ""Room 1""
          },
          {
            ""id"": ""98310"",
            ""title"": ""A Skeptics Guide to Graph Databases"",
            ""description"": ""Graph databases are one of the hottest trends in tech, but is it hype or can they actually solve real problems?  Well, the answer is both.\r\n\r\nIn this talk, Dave will pull back the covers and show you the good, the bad, and the ugly of solving real problems with graph databases.  He will demonstrate how you can leverage the power of graph databases to solve difficult problems or existing problems differently.  He will then discuss when to avoid them and just use your favorite RDBMS. We will then examine a few of his failures so that we can all learn from his mistakes.  By the end of this talk, you will either be excited to use a graph database or run away screaming, either way, you will be armed with the information you need to cut through the hype and know when to use one and when to avoid them."",
            ""startsAt"": ""2019-06-20T15:00:00"",
            ""endsAt"": ""2019-06-20T16:00:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""6865759f-cb01-408c-8267-1f9af62f84ee"",
                ""name"": ""David Bechberger""
              }
            ],
            ""categories"": [],
            ""roomId"": 4479,
            ""room"": ""Room 1""
          },
          {
            ""id"": ""76922"",
            ""title"": ""ML and the IoT: Living on the Edge"",
            ""description"": ""Machine Learning and the IoT are a match made in heaven. After all, IoT devices collect mountains of sensor data, what better way to uncover insights and actions than through sophisticated, modern computing methods like ML and AI?\r\n\r\nThe problem is, leveraging ML with IoT has historically meant backhauling all your sensor data to the Cloud. When the cloud is involved, security is a concern, and in the realm of IoT, security is often a dirty word.\r\n\r\nBut modern embedded systems, microcontrollers and single-board computers are getting more powerful, and more sophisticated, and its becoming increasingly possible to bring Machine Learning closer to sensors and IoT devices. \""Edge ML\"" enables quicker insights, tighter security, and even true predictive action, and it's going to become the norm in the IoT in the near future.\r\n\r\nIn this session, we'll explore the state of the art in Edge ML and IoT, and talk about practical ways that developers can get started with both, today."",
            ""startsAt"": ""2019-06-20T16:20:00"",
            ""endsAt"": ""2019-06-20T17:20:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""f1547f29-3799-4bdb-bcbf-112a4e11253e"",
                ""name"": ""Brandon Satrom""
              }
            ],
            ""categories"": [],
            ""roomId"": 4479,
            ""room"": ""Room 1""
          },
          {
            ""id"": ""84573"",
            ""title"": ""Tick Tock: What the heck is time-series data?"",
            ""description"": ""The rise of IoT and smart infrastructure has led to the generation of massive amounts of complex data. In this session, we will talk about time-series data, the challenges of working with time series data, ingestion of this data using data from NYC cabs and running real time queries to gather insights. By the end of the session, we will have an understanding of what time-series data is, how to build streaming data pipelines for massive time series data using Flink, Kafka and CrateDB, and visualising all this data with the help of a dashboard."",
            ""startsAt"": ""2019-06-20T17:40:00"",
            ""endsAt"": ""2019-06-20T18:40:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""2c535d4d-198c-4132-932c-bebe9a948306"",
                ""name"": ""Tanay Pant""
              }
            ],
            ""categories"": [],
            ""roomId"": 4479,
            ""room"": ""Room 1""
          }
        ],
        ""hasOnlyPlenumSessions"": false
      },
      {
        ""id"": 4480,
        ""name"": ""Room 2"",
        ""sessions"": [
          {
            ""id"": ""86264"",
            ""title"": ""Kotlin for C# Developers"",
            ""description"": ""Dive into the latest craze in languages and platforms - Kotlin. This time we will be looking at it from the perspective of a .NET C# developer, draw comparisons between the languages, and bridge the gap between these 2 amazing languages.\r\n\r\nWe'll look at:\r\n- Kotlin as a language\r\n- Platforms Kotlin is great for\r\n- Object Oriented Implementations in Kotlin\r\n- Extended Features\r\n- Features Kotlin has that C# doesn't\r\n- A demo Android application in Kotlin vs a Xamarin.Android app in C#\r\n\r\nIn the end you will leave with a foundational knowledge of Kotlin and its capabilities to build awesome apps with less code. You should feel comfortable comparing C# applications to Kotlin applications and know where to find resources to learn even more!"",
            ""startsAt"": ""2019-06-20T09:00:00"",
            ""endsAt"": ""2019-06-20T10:00:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""42b758e4-1a7c-434d-9951-ddff53503d1f"",
                ""name"": ""Alex Dunn""
              }
            ],
            ""categories"": [],
            ""roomId"": 4480,
            ""room"": ""Room 2""
          },
          {
            ""id"": ""99197"",
            ""title"": ""Kotlin coroutines: new ways to do asynchronous programming"",
            ""description"": ""The async/await feature allows you to write the asynchronous code in a straightforward way, without a long list of callbacks. Used in C# for quite a while already, it has proven to be extremely useful. In Kotlin you have async and await as library functions implemented using coroutines.\r\n\r\nA coroutine is a light-weight thread that can be suspended and resumed later. Very precise definition, but might be confusing at first. What 'light-weight thread' means? How does suspension work? This talk uncovers the magic.\r\nWe'll discuss the concept of coroutines, the power of async/await, and how you can benefit from defining your asynchronous computations using suspend functions."",
            ""startsAt"": ""2019-06-20T10:20:00"",
            ""endsAt"": ""2019-06-20T11:20:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""0f769b19-d5f2-49fb-aa78-a086aa046b7e"",
                ""name"": ""Svetlana Isakova""
              }
            ],
            ""categories"": [],
            ""roomId"": 4480,
            ""room"": ""Room 2""
          },
          {
            ""id"": ""78448"",
            ""title"": ""Developing Kernel Drivers with Modern C++"",
            ""description"": ""Kernel drivers are traditionally written in C, but today drivers can be built with the latest C++ standards. The session presents examples and best practices when developing kernel code with C++"",
            ""startsAt"": ""2019-06-20T11:40:00"",
            ""endsAt"": ""2019-06-20T12:40:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""3b2b49cf-5746-484c-a85e-be960fe76043"",
                ""name"": ""Pavel Yosifovich""
              }
            ],
            ""categories"": [],
            ""roomId"": 4480,
            ""room"": ""Room 2""
          },
          {
            ""id"": ""81208"",
            ""title"": ""Accessibility: Coding and Beyond"",
            ""description"": ""Accessibility is for everyone and is a responsibility of all team members. While a lot of web accessibility guidelines are focused on coding, there are other accessibility components that all team members need to consider in order to improve the overall product or service from the beginning of any projects. Sveta will share her personal experience as a deaf person and some examples of accessibility issues and solutions. They may be new to some and sound like common sense to others, but sadly there are many products and services that fail at accessibility. The talk will help developers and their team members better understand why accessibility is not just about coding and why it's not to be used as an afterthought."",
            ""startsAt"": ""2019-06-20T13:40:00"",
            ""endsAt"": ""2019-06-20T14:40:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""ddff421a-de84-4d8f-9d2a-e799ea9c5055"",
                ""name"": ""Svetlana Kouznetsova""
              }
            ],
            ""categories"": [],
            ""roomId"": 4480,
            ""room"": ""Room 2""
          },
          {
            ""id"": ""127292"",
            ""title"": ""Deliberate Architecture"",
            ""description"": ""Step back from your system and take a look at its architecture. Are the major structures and technology choices the result of conscious decisions, or have they emerged as the system has evolved? Architecture is often an ad hoc, responsive process where designs get stuck in local minima while ever more features are piled into the system. Such systems often fail to live up to the origin vision and expectations of stakeholders.\r\n\r\nIn this talk we look at how to design systems which are a purely a function of the major forces acting on a solution, rather than being modishly reflective of the prevailing software zeitgeist. We’ll explore the idea that software architecture, and hence software architects, should focus deliberately on the constraints and qualities of system design, and avoid getting too distracted by features."",
            ""startsAt"": ""2019-06-20T15:00:00"",
            ""endsAt"": ""2019-06-20T16:00:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""fb8c0c65-88de-4673-b049-0e8a385c89ad"",
                ""name"": ""Robert Smallshire""
              }
            ],
            ""categories"": [],
            ""roomId"": 4480,
            ""room"": ""Room 2""
          },
          {
            ""id"": ""98925"",
            ""title"": ""The Curiously Recurring Pattern of Coupled Types"",
            ""description"": ""Why can pointers be subtracted but not added?\r\n\r\nWhat do raw C pointers, STL iterators, std::chrono types, and 2D/3D geometric primitives have in common?\r\n\r\nIn this talk we will present some curiously coupled data types that frequently occur in your programs, together forming notions that you are already intuitively familiar with. We will shine a light on the mathematical notion of Affine Spaces, and how they guide stronger design. We will review the properties of affine spaces and show how they improve program semantics, stronger type safety and compile time enforcement of these semantics.\r\n\r\nBy showing motivational examples, we will introduce you to the mathematical notion of affine spaces. The main focus will then be on how affine space types and their well defined semantics shape expressive APIs.\r\n\r\nWe will give examples and guidelines for creating your own affine types. Although the examples in the talk will use C++, the general concepts are applicable to other strongly typed  programming languages.\r\n"",
            ""startsAt"": ""2019-06-20T16:20:00"",
            ""endsAt"": ""2019-06-20T17:20:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""f90c9365-b46d-41ef-bfe8-5a8ce7869ab5"",
                ""name"": ""Adi Shavit""
              },
              {
                ""id"": ""faf284c6-cee1-4e2c-bd6c-91e92ad33400"",
                ""name"": ""Björn Fahller""
              }
            ],
            ""categories"": [],
            ""roomId"": 4480,
            ""room"": ""Room 2""
          },
          {
            ""id"": ""99530"",
            ""title"": "".NET Rocks Live!"",
            ""description"": ""Migrating from WCF to .NET Core with Mark Rendle\r\nAre you looking to migrate off of WCF? Microsoft has said that WCF will not move to .NET Core – so what are your options? \r\n\r\nJoin Carl and Richard from .NET Rocks as they talk to Mark Rendle about his work building a tool to help migrate WCF applications to a combination of .NET Core, ASP.NET Web API and gRPC. Bring your questions and be part of an in-person .NET Rocks recording!"",
            ""startsAt"": ""2019-06-20T17:40:00"",
            ""endsAt"": ""2019-06-20T18:40:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""7a34a7fc-2f1c-4751-af4f-18a21de0e6b3"",
                ""name"": ""Richard Campbell""
              },
              {
                ""id"": ""6d1065d2-3494-48a6-a6bb-7efde3d8de40"",
                ""name"": ""Carl Franklin""
              },
              {
                ""id"": ""70590ad0-b1b3-40b8-b05d-b58722ef9d9d"",
                ""name"": ""Mark Rendle""
              }
            ],
            ""categories"": [],
            ""roomId"": 4480,
            ""room"": ""Room 2""
          }
        ],
        ""hasOnlyPlenumSessions"": false
      },
      {
        ""id"": 4481,
        ""name"": ""Room 3"",
        ""sessions"": [
          {
            ""id"": ""99054"",
            ""title"": ""How to use ML.NET to write crushing metal riffs"",
            ""description"": ""ML.NET is still fairly fresh Microsoft venture into deep learning. It's written in .NET Core and a lot of good thinking went into it. It's definitely the best hope for .NET developers to do machine learning natively and easily incorporate it into existing apps. \r\n\r\nAnd to show its power we'll harness deep learning to help a metal band to get out of their rut by writing for them some awesome new riffs. \r\n\r\nFrom this talk, you'll learn how to use ML.NET, how some deep learning techniques like Recurrent Neural Networks work and how to write metal songs!\r\n"",
            ""startsAt"": ""2019-06-20T09:00:00"",
            ""endsAt"": ""2019-06-20T10:00:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""1cfbd2b6-db6f-405a-a997-e0c04e5f1510"",
                ""name"": ""Michał Łusiak""
              }
            ],
            ""categories"": [],
            ""roomId"": 4481,
            ""room"": ""Room 3""
          },
          {
            ""id"": ""86691"",
            ""title"": ""An Introduction to WebAssembly"",
            ""description"": ""Want to write a web application? Better get familiar with JavaScript! JavaScript has long been the king of front-end. While there have been various attempts to dethrone it, they have typically involved treating JavaScript as an assembly-language analog that you transpile your code to. This has lead to complex build pipelines that result in JavaScript which the browser has to parse and *you* still have to debug. But what if there were an actual byte-code language you could compile your non-JavaScript code to instead? That is what WebAssembly is.\r\n\r\nI'm going to explain how WebAssembly works and how to use it in this talk. I'll cover what it is, how it fits into your application, and how to build and use your own WebAssembly modules. And, I'll demo how to build and use those modules with Rust, C++, and the WebAssembly Text Format. That's right, I'll be live coding in an assembly language. I'll also go over some online resources for other languages and tools that make use of WebAssembly.\r\n\r\nWhen we're done, you'll have the footing you need to start building applications featuring WebAssembly. So grab a non-JavaScript language, a modern browser, and let's and get started!"",
            ""startsAt"": ""2019-06-20T10:20:00"",
            ""endsAt"": ""2019-06-20T11:20:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""520bf4ca-b2d3-47c3-8475-c25bb2b257f7"",
                ""name"": ""Guy Royse""
              }
            ],
            ""categories"": [],
            ""roomId"": 4481,
            ""room"": ""Room 3""
          },
          {
            ""id"": ""99526"",
            ""title"": ""Pointless or Pointfree?"",
            ""description"": ""With the emergence of functional programming in mainstream JavaScript, point-free programming style (also known as tacit programming) is gaining traction. If you ever used RxJS you'll know what I'm talking about...\r\n\r\nIn this talk, we'll explore the motivation behind it and discover some interesting consequences. After imposing a strict set of rules, we'll try and solve a small practical problem and see where does that take us. This talk is a mix of presentation and live coding with examples in JavaScript."",
            ""startsAt"": ""2019-06-20T11:40:00"",
            ""endsAt"": ""2019-06-20T12:40:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""c51fb01c-63d1-4b84-92d0-231f1fe3673c"",
                ""name"": ""Damjan Vujnovic""
              }
            ],
            ""categories"": [],
            ""roomId"": 4481,
            ""room"": ""Room 3""
          },
          {
            ""id"": ""99250"",
            ""title"": ""The Seven Deadly Presentation Sins"",
            ""description"": ""What makes a great presentation? More importantly, what are the elements that can destroy a great presentation, even if the content itself is technically sound? \r\n\r\nIn this session Samantha and Andrew Coates demonstrate seven sins that must not be committed in a presentation, why and how a presentation can suffer from committing them, and how to avoid accidently committing them. \r\n\r\nIronically, when we practice we deliberately commit presentation sins. We stop, we fix, we repeat and refine. Like elite musicians, dedicated technical presenters typically spend many solitary hours doing this, which of course is necessary to master any content. But none of these practice processes has a place on the presentation stage. In presentation we must trust, believe, create, and keep going in the face of any adversity – a completely different process which in itself must be rehearsed.  \r\nIn order to give a magnificent presentation, we must PRACTICE giving a magnificent presentation. The only way to do this is by regularly and deliberately creating pressure situations in which we can practice NOT committing the presentation sins.\r\n\r\nCovering everything from wrong notes to blue screens of death, this dynamic and multi-talented pair not only break down the elements of successful and unsuccessful presentations, but also outline how one can give a truly engaging presentation every time, no matter how basic or complex the content."",
            ""startsAt"": ""2019-06-20T13:40:00"",
            ""endsAt"": ""2019-06-20T14:40:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""2809c750-dc83-4e1c-8f06-38ee96b818b6"",
                ""name"": ""Andrew Coates""
              },
              {
                ""id"": ""0e2bfca8-149b-49f8-88d4-bd6558e8f11e"",
                ""name"": ""Samantha Coates""
              }
            ],
            ""categories"": [],
            ""roomId"": 4481,
            ""room"": ""Room 3""
          },
          {
            ""id"": ""77944"",
            ""title"": ""Empathetic Design Systems"",
            ""description"": ""How do you make a design system empathetic and whom should it be empathetic towards? At a recent company, we decided to replace our outdated style guide with a newfangled design system that we started from scratch. And we made a great start.\r\n\r\nBut we forgot about accessibility. Only stand alone components reflected the basics of accessibility leaving full user flows behind. We forgot about our fellow coworkers and peers. Our engineers shouldered slow development times and new technologies, designs changed often, and variants were hard to implement. And we forgot about our users. Much of the design system was geared towards engineers, neglecting designers, product managers and more.\r\n\r\nSo what did we learn in our first iteration? How did empathy help shape our ever-changing, morphing design system? Come learn how to build an empathetic design system from the ground up or start incorporating empathy today!\r\n"",
            ""startsAt"": ""2019-06-20T15:00:00"",
            ""endsAt"": ""2019-06-20T16:00:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""cdc97666-b7bb-4c51-aec5-c73095c08c54"",
                ""name"": ""Jennifer Wong""
              }
            ],
            ""categories"": [],
            ""roomId"": 4481,
            ""room"": ""Room 3""
          },
          {
            ""id"": ""127293"",
            ""title"": ""Anchored Metadata"",
            ""description"": ""When building software, we often need to associate metadata with the code we’re writing. A typical example is when we need to tell our linters to ignore a specific range of code. A common approach to adding this metadata is to embed it directly in the code using the syntax of the language, but this approach has a number of drawbacks including language specificity, potential for collision, and cluttering of the code.\r\n\r\nIn this talk we’ll look at an alternative approach that stores the metadata separate from the code using a technique called _anchoring_. The metadata is associated with an anchor, a region of code inside the source file. Critically, the anchor also includes a context, a snapshot of the code surrounding the anchored region. As the source code is changed, this context – along with some very interesting algorithms for aligning text -– is used to automatically update the anchors.\r\n\r\nTo demonstrate these concepts we’ll look at spor, a tool that implements anchoring and anchor updating. The primary implementation of spor is in Python, so it’s very approachable and, indeed, open for contribution. As a side note, we’ll also look at a partial implementation of spor written in Rust. Finally, we’ll look at how spor is being used in Cosmic Ray, a mutation testing tool for Python."",
            ""startsAt"": ""2019-06-20T16:20:00"",
            ""endsAt"": ""2019-06-20T17:20:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""ad6fb1a9-9b48-4393-95f1-f352e643c541"",
                ""name"": ""Austin Bingham""
              }
            ],
            ""categories"": [],
            ""roomId"": 4481,
            ""room"": ""Room 3""
          },
          {
            ""id"": ""127294"",
            ""title"": ""UX Design Fundamentals: What do your users really see"",
            ""description"": ""Developers are often unaware of how their users actually see their screens. In this UX design session, we'll discuss the most important principles concerning how the human brain and visual system determine how users see application interfaces.\r\n\r\nWe'll look at Gestalt principles for grouping and highlighting, inattentional blindness and change blindness, how users scan through a view, and how to promote clarity in interfaces with levels of emphasis. Tests will help attendees see how they personally experience these principles, and better understand the challenges faced by their users when views and pages are not designed to respect design principles."",
            ""startsAt"": ""2019-06-20T17:40:00"",
            ""endsAt"": ""2019-06-20T18:40:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""af1eed12-4400-4d1c-885f-7fbb41e737b6"",
                ""name"": ""Billy Hollis""
              }
            ],
            ""categories"": [],
            ""roomId"": 4481,
            ""room"": ""Room 3""
          }
        ],
        ""hasOnlyPlenumSessions"": false
      },
      {
        ""id"": 4489,
        ""name"": ""Expo"",
        ""sessions"": [
          {
            ""id"": ""128960"",
            ""title"": ""Party"",
            ""description"": ""Kick back, get to know your fellow attendees and enjoy live music from the main stage. The party is complimentary for all NDC delegates. (18:40- 23:00)\r\n\r\nSchedule:\r\n\r\n- Conference Reception in the Expo\r\n- Fun talks with host Lars Klint\r\n- Dylan Beattie and the Linebreakers\r\n- LoveShack"",
            ""startsAt"": ""2019-06-20T18:40:00"",
            ""endsAt"": ""2019-06-20T23:00:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""f99fa878-6176-4ff4-b151-c717fa5daf0c"",
                ""name"": ""Lars Klint""
              },
              {
                ""id"": ""1d7dcbfc-1de6-4228-8bd6-04f4ba1c4267"",
                ""name"": ""Dylan Beattie""
              }
            ],
            ""categories"": [],
            ""roomId"": 4489,
            ""room"": ""Expo""
          }
        ],
        ""hasOnlyPlenumSessions"": false
      }
    ],
    ""timeSlots"": [
      {
        ""slotStart"": ""09:00:00"",
        ""rooms"": [
          {
            ""id"": 4479,
            ""name"": ""Room 1"",
            ""session"": {
              ""id"": ""74808"",
              ""title"": ""Getting Started with Cosmos DB + EF Core"",
              ""description"": ""Cosmos DB is great and awesomely fast. Wouldn't be even more amazing if we could use our beloved entity framework to manage it? Let see how we can wire it up and get started"",
              ""startsAt"": ""2019-06-20T09:00:00"",
              ""endsAt"": ""2019-06-20T10:00:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""c739e2f1-ecf5-43e1-abcf-90cf13dd7b8f"",
                  ""name"": ""Thiago Passos""
                }
              ],
              ""categories"": [],
              ""roomId"": 4479,
              ""room"": ""Room 1""
            },
            ""index"": 0
          },
          {
            ""id"": 4480,
            ""name"": ""Room 2"",
            ""session"": {
              ""id"": ""86264"",
              ""title"": ""Kotlin for C# Developers"",
              ""description"": ""Dive into the latest craze in languages and platforms - Kotlin. This time we will be looking at it from the perspective of a .NET C# developer, draw comparisons between the languages, and bridge the gap between these 2 amazing languages.\r\n\r\nWe'll look at:\r\n- Kotlin as a language\r\n- Platforms Kotlin is great for\r\n- Object Oriented Implementations in Kotlin\r\n- Extended Features\r\n- Features Kotlin has that C# doesn't\r\n- A demo Android application in Kotlin vs a Xamarin.Android app in C#\r\n\r\nIn the end you will leave with a foundational knowledge of Kotlin and its capabilities to build awesome apps with less code. You should feel comfortable comparing C# applications to Kotlin applications and know where to find resources to learn even more!"",
              ""startsAt"": ""2019-06-20T09:00:00"",
              ""endsAt"": ""2019-06-20T10:00:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""42b758e4-1a7c-434d-9951-ddff53503d1f"",
                  ""name"": ""Alex Dunn""
                }
              ],
              ""categories"": [],
              ""roomId"": 4480,
              ""room"": ""Room 2""
            },
            ""index"": 1
          },
          {
            ""id"": 4481,
            ""name"": ""Room 3"",
            ""session"": {
              ""id"": ""99054"",
              ""title"": ""How to use ML.NET to write crushing metal riffs"",
              ""description"": ""ML.NET is still fairly fresh Microsoft venture into deep learning. It's written in .NET Core and a lot of good thinking went into it. It's definitely the best hope for .NET developers to do machine learning natively and easily incorporate it into existing apps. \r\n\r\nAnd to show its power we'll harness deep learning to help a metal band to get out of their rut by writing for them some awesome new riffs. \r\n\r\nFrom this talk, you'll learn how to use ML.NET, how some deep learning techniques like Recurrent Neural Networks work and how to write metal songs!\r\n"",
              ""startsAt"": ""2019-06-20T09:00:00"",
              ""endsAt"": ""2019-06-20T10:00:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""1cfbd2b6-db6f-405a-a997-e0c04e5f1510"",
                  ""name"": ""Michał Łusiak""
                }
              ],
              ""categories"": [],
              ""roomId"": 4481,
              ""room"": ""Room 3""
            },
            ""index"": 2
          }
        ]
      },
      {
        ""slotStart"": ""10:20:00"",
        ""rooms"": [
          {
            ""id"": 4479,
            ""name"": ""Room 1"",
            ""session"": {
              ""id"": ""98994"",
              ""title"": ""Deep Learning in the world of little ponies"",
              ""description"": ""In this talk, we will discuss computer vision, one of the most common real-world applications of machine learning. We will deep dive into various state-of-the-art concepts around building custom image classifiers - application of deep neural networks, specifically convolutional neural networks and transfer learning. We will demonstrate how those approaches could be used to create your own image classifier to recognise the characters of \""My Little Pony\"" TV Series [or Pokemon, or Superheroes, or your custom images]."",
              ""startsAt"": ""2019-06-20T10:20:00"",
              ""endsAt"": ""2019-06-20T11:20:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""b4c506ac-9757-4ac6-8b7c-3d6696b113e4"",
                  ""name"": ""Galiya Warrier""
                }
              ],
              ""categories"": [],
              ""roomId"": 4479,
              ""room"": ""Room 1""
            },
            ""index"": 0
          },
          {
            ""id"": 4480,
            ""name"": ""Room 2"",
            ""session"": {
              ""id"": ""99197"",
              ""title"": ""Kotlin coroutines: new ways to do asynchronous programming"",
              ""description"": ""The async/await feature allows you to write the asynchronous code in a straightforward way, without a long list of callbacks. Used in C# for quite a while already, it has proven to be extremely useful. In Kotlin you have async and await as library functions implemented using coroutines.\r\n\r\nA coroutine is a light-weight thread that can be suspended and resumed later. Very precise definition, but might be confusing at first. What 'light-weight thread' means? How does suspension work? This talk uncovers the magic.\r\nWe'll discuss the concept of coroutines, the power of async/await, and how you can benefit from defining your asynchronous computations using suspend functions."",
              ""startsAt"": ""2019-06-20T10:20:00"",
              ""endsAt"": ""2019-06-20T11:20:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""0f769b19-d5f2-49fb-aa78-a086aa046b7e"",
                  ""name"": ""Svetlana Isakova""
                }
              ],
              ""categories"": [],
              ""roomId"": 4480,
              ""room"": ""Room 2""
            },
            ""index"": 1
          },
          {
            ""id"": 4481,
            ""name"": ""Room 3"",
            ""session"": {
              ""id"": ""86691"",
              ""title"": ""An Introduction to WebAssembly"",
              ""description"": ""Want to write a web application? Better get familiar with JavaScript! JavaScript has long been the king of front-end. While there have been various attempts to dethrone it, they have typically involved treating JavaScript as an assembly-language analog that you transpile your code to. This has lead to complex build pipelines that result in JavaScript which the browser has to parse and *you* still have to debug. But what if there were an actual byte-code language you could compile your non-JavaScript code to instead? That is what WebAssembly is.\r\n\r\nI'm going to explain how WebAssembly works and how to use it in this talk. I'll cover what it is, how it fits into your application, and how to build and use your own WebAssembly modules. And, I'll demo how to build and use those modules with Rust, C++, and the WebAssembly Text Format. That's right, I'll be live coding in an assembly language. I'll also go over some online resources for other languages and tools that make use of WebAssembly.\r\n\r\nWhen we're done, you'll have the footing you need to start building applications featuring WebAssembly. So grab a non-JavaScript language, a modern browser, and let's and get started!"",
              ""startsAt"": ""2019-06-20T10:20:00"",
              ""endsAt"": ""2019-06-20T11:20:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""520bf4ca-b2d3-47c3-8475-c25bb2b257f7"",
                  ""name"": ""Guy Royse""
                }
              ],
              ""categories"": [],
              ""roomId"": 4481,
              ""room"": ""Room 3""
            },
            ""index"": 2
          }
        ]
      },
      {
        ""slotStart"": ""11:40:00"",
        ""rooms"": [
          {
            ""id"": 4479,
            ""name"": ""Room 1"",
            ""session"": {
              ""id"": ""80032"",
              ""title"": ""Living in eventually consistent reality"",
              ""description"": ""Strongly consistent databases are dominating world of software. However, with increasing scale and global availability of our services, many developers often prefer to loose their constraints in favor of an eventual consistency. \r\n\r\nDuring this presentation we'll talk about Conflict-free Replicated Data Types (CRDT) - an eventually-consistent structures, that can be found in many modern day multi-master, geo-distributed databases such as CosmosDB, DynamoDB, Riak, Cassandra or Redis: how do they work and what makes them so interesting choice in highly available systems."",
              ""startsAt"": ""2019-06-20T11:40:00"",
              ""endsAt"": ""2019-06-20T12:40:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""6373f8a6-11a6-43e9-b1ee-a898dd02d1ce"",
                  ""name"": ""Bartosz Sypytkowski""
                }
              ],
              ""categories"": [],
              ""roomId"": 4479,
              ""room"": ""Room 1""
            },
            ""index"": 0
          },
          {
            ""id"": 4480,
            ""name"": ""Room 2"",
            ""session"": {
              ""id"": ""78448"",
              ""title"": ""Developing Kernel Drivers with Modern C++"",
              ""description"": ""Kernel drivers are traditionally written in C, but today drivers can be built with the latest C++ standards. The session presents examples and best practices when developing kernel code with C++"",
              ""startsAt"": ""2019-06-20T11:40:00"",
              ""endsAt"": ""2019-06-20T12:40:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""3b2b49cf-5746-484c-a85e-be960fe76043"",
                  ""name"": ""Pavel Yosifovich""
                }
              ],
              ""categories"": [],
              ""roomId"": 4480,
              ""room"": ""Room 2""
            },
            ""index"": 1
          },
          {
            ""id"": 4481,
            ""name"": ""Room 3"",
            ""session"": {
              ""id"": ""99526"",
              ""title"": ""Pointless or Pointfree?"",
              ""description"": ""With the emergence of functional programming in mainstream JavaScript, point-free programming style (also known as tacit programming) is gaining traction. If you ever used RxJS you'll know what I'm talking about...\r\n\r\nIn this talk, we'll explore the motivation behind it and discover some interesting consequences. After imposing a strict set of rules, we'll try and solve a small practical problem and see where does that take us. This talk is a mix of presentation and live coding with examples in JavaScript."",
              ""startsAt"": ""2019-06-20T11:40:00"",
              ""endsAt"": ""2019-06-20T12:40:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""c51fb01c-63d1-4b84-92d0-231f1fe3673c"",
                  ""name"": ""Damjan Vujnovic""
                }
              ],
              ""categories"": [],
              ""roomId"": 4481,
              ""room"": ""Room 3""
            },
            ""index"": 2
          }
        ]
      },
      {
        ""slotStart"": ""13:40:00"",
        ""rooms"": [
          {
            ""id"": 4479,
            ""name"": ""Room 1"",
            ""session"": {
              ""id"": ""98933"",
              ""title"": ""Is AI right for me?"",
              ""description"": ""Artificial intelligence (AI) has become a form of Swiss Army knife for the enterprise world. If you have a data problem, throw some AI at it! However, this mentality can lead to wasted time and money going down the path of implementing a heavy-handed solution that doesn’t fit your business problem. Navigating the waters of AI, machine learning and data analysis can be tricky, especially when being sold by the myriad of data science and AI companies offering solutions at the enterprise level. But fear not, some simple guidelines can help. In this talk, I will present a basic rubric for evaluating AI and data analytics techniques as potential solutions for enterprise business problems."",
              ""startsAt"": ""2019-06-20T13:40:00"",
              ""endsAt"": ""2019-06-20T14:40:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""e1864dca-8a88-4ea7-840a-20b88702b38f"",
                  ""name"": ""Amber McKenzie""
                }
              ],
              ""categories"": [],
              ""roomId"": 4479,
              ""room"": ""Room 1""
            },
            ""index"": 0
          },
          {
            ""id"": 4480,
            ""name"": ""Room 2"",
            ""session"": {
              ""id"": ""81208"",
              ""title"": ""Accessibility: Coding and Beyond"",
              ""description"": ""Accessibility is for everyone and is a responsibility of all team members. While a lot of web accessibility guidelines are focused on coding, there are other accessibility components that all team members need to consider in order to improve the overall product or service from the beginning of any projects. Sveta will share her personal experience as a deaf person and some examples of accessibility issues and solutions. They may be new to some and sound like common sense to others, but sadly there are many products and services that fail at accessibility. The talk will help developers and their team members better understand why accessibility is not just about coding and why it's not to be used as an afterthought."",
              ""startsAt"": ""2019-06-20T13:40:00"",
              ""endsAt"": ""2019-06-20T14:40:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""ddff421a-de84-4d8f-9d2a-e799ea9c5055"",
                  ""name"": ""Svetlana Kouznetsova""
                }
              ],
              ""categories"": [],
              ""roomId"": 4480,
              ""room"": ""Room 2""
            },
            ""index"": 1
          },
          {
            ""id"": 4481,
            ""name"": ""Room 3"",
            ""session"": {
              ""id"": ""99250"",
              ""title"": ""The Seven Deadly Presentation Sins"",
              ""description"": ""What makes a great presentation? More importantly, what are the elements that can destroy a great presentation, even if the content itself is technically sound? \r\n\r\nIn this session Samantha and Andrew Coates demonstrate seven sins that must not be committed in a presentation, why and how a presentation can suffer from committing them, and how to avoid accidently committing them. \r\n\r\nIronically, when we practice we deliberately commit presentation sins. We stop, we fix, we repeat and refine. Like elite musicians, dedicated technical presenters typically spend many solitary hours doing this, which of course is necessary to master any content. But none of these practice processes has a place on the presentation stage. In presentation we must trust, believe, create, and keep going in the face of any adversity – a completely different process which in itself must be rehearsed.  \r\nIn order to give a magnificent presentation, we must PRACTICE giving a magnificent presentation. The only way to do this is by regularly and deliberately creating pressure situations in which we can practice NOT committing the presentation sins.\r\n\r\nCovering everything from wrong notes to blue screens of death, this dynamic and multi-talented pair not only break down the elements of successful and unsuccessful presentations, but also outline how one can give a truly engaging presentation every time, no matter how basic or complex the content."",
              ""startsAt"": ""2019-06-20T13:40:00"",
              ""endsAt"": ""2019-06-20T14:40:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""2809c750-dc83-4e1c-8f06-38ee96b818b6"",
                  ""name"": ""Andrew Coates""
                },
                {
                  ""id"": ""0e2bfca8-149b-49f8-88d4-bd6558e8f11e"",
                  ""name"": ""Samantha Coates""
                }
              ],
              ""categories"": [],
              ""roomId"": 4481,
              ""room"": ""Room 3""
            },
            ""index"": 2
          }
        ]
      },
      {
        ""slotStart"": ""15:00:00"",
        ""rooms"": [
          {
            ""id"": 4479,
            ""name"": ""Room 1"",
            ""session"": {
              ""id"": ""98310"",
              ""title"": ""A Skeptics Guide to Graph Databases"",
              ""description"": ""Graph databases are one of the hottest trends in tech, but is it hype or can they actually solve real problems?  Well, the answer is both.\r\n\r\nIn this talk, Dave will pull back the covers and show you the good, the bad, and the ugly of solving real problems with graph databases.  He will demonstrate how you can leverage the power of graph databases to solve difficult problems or existing problems differently.  He will then discuss when to avoid them and just use your favorite RDBMS. We will then examine a few of his failures so that we can all learn from his mistakes.  By the end of this talk, you will either be excited to use a graph database or run away screaming, either way, you will be armed with the information you need to cut through the hype and know when to use one and when to avoid them."",
              ""startsAt"": ""2019-06-20T15:00:00"",
              ""endsAt"": ""2019-06-20T16:00:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""6865759f-cb01-408c-8267-1f9af62f84ee"",
                  ""name"": ""David Bechberger""
                }
              ],
              ""categories"": [],
              ""roomId"": 4479,
              ""room"": ""Room 1""
            },
            ""index"": 0
          },
          {
            ""id"": 4480,
            ""name"": ""Room 2"",
            ""session"": {
              ""id"": ""127292"",
              ""title"": ""Deliberate Architecture"",
              ""description"": ""Step back from your system and take a look at its architecture. Are the major structures and technology choices the result of conscious decisions, or have they emerged as the system has evolved? Architecture is often an ad hoc, responsive process where designs get stuck in local minima while ever more features are piled into the system. Such systems often fail to live up to the origin vision and expectations of stakeholders.\r\n\r\nIn this talk we look at how to design systems which are a purely a function of the major forces acting on a solution, rather than being modishly reflective of the prevailing software zeitgeist. We’ll explore the idea that software architecture, and hence software architects, should focus deliberately on the constraints and qualities of system design, and avoid getting too distracted by features."",
              ""startsAt"": ""2019-06-20T15:00:00"",
              ""endsAt"": ""2019-06-20T16:00:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""fb8c0c65-88de-4673-b049-0e8a385c89ad"",
                  ""name"": ""Robert Smallshire""
                }
              ],
              ""categories"": [],
              ""roomId"": 4480,
              ""room"": ""Room 2""
            },
            ""index"": 1
          },
          {
            ""id"": 4481,
            ""name"": ""Room 3"",
            ""session"": {
              ""id"": ""77944"",
              ""title"": ""Empathetic Design Systems"",
              ""description"": ""How do you make a design system empathetic and whom should it be empathetic towards? At a recent company, we decided to replace our outdated style guide with a newfangled design system that we started from scratch. And we made a great start.\r\n\r\nBut we forgot about accessibility. Only stand alone components reflected the basics of accessibility leaving full user flows behind. We forgot about our fellow coworkers and peers. Our engineers shouldered slow development times and new technologies, designs changed often, and variants were hard to implement. And we forgot about our users. Much of the design system was geared towards engineers, neglecting designers, product managers and more.\r\n\r\nSo what did we learn in our first iteration? How did empathy help shape our ever-changing, morphing design system? Come learn how to build an empathetic design system from the ground up or start incorporating empathy today!\r\n"",
              ""startsAt"": ""2019-06-20T15:00:00"",
              ""endsAt"": ""2019-06-20T16:00:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""cdc97666-b7bb-4c51-aec5-c73095c08c54"",
                  ""name"": ""Jennifer Wong""
                }
              ],
              ""categories"": [],
              ""roomId"": 4481,
              ""room"": ""Room 3""
            },
            ""index"": 2
          }
        ]
      },
      {
        ""slotStart"": ""16:20:00"",
        ""rooms"": [
          {
            ""id"": 4479,
            ""name"": ""Room 1"",
            ""session"": {
              ""id"": ""76922"",
              ""title"": ""ML and the IoT: Living on the Edge"",
              ""description"": ""Machine Learning and the IoT are a match made in heaven. After all, IoT devices collect mountains of sensor data, what better way to uncover insights and actions than through sophisticated, modern computing methods like ML and AI?\r\n\r\nThe problem is, leveraging ML with IoT has historically meant backhauling all your sensor data to the Cloud. When the cloud is involved, security is a concern, and in the realm of IoT, security is often a dirty word.\r\n\r\nBut modern embedded systems, microcontrollers and single-board computers are getting more powerful, and more sophisticated, and its becoming increasingly possible to bring Machine Learning closer to sensors and IoT devices. \""Edge ML\"" enables quicker insights, tighter security, and even true predictive action, and it's going to become the norm in the IoT in the near future.\r\n\r\nIn this session, we'll explore the state of the art in Edge ML and IoT, and talk about practical ways that developers can get started with both, today."",
              ""startsAt"": ""2019-06-20T16:20:00"",
              ""endsAt"": ""2019-06-20T17:20:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""f1547f29-3799-4bdb-bcbf-112a4e11253e"",
                  ""name"": ""Brandon Satrom""
                }
              ],
              ""categories"": [],
              ""roomId"": 4479,
              ""room"": ""Room 1""
            },
            ""index"": 0
          },
          {
            ""id"": 4480,
            ""name"": ""Room 2"",
            ""session"": {
              ""id"": ""98925"",
              ""title"": ""The Curiously Recurring Pattern of Coupled Types"",
              ""description"": ""Why can pointers be subtracted but not added?\r\n\r\nWhat do raw C pointers, STL iterators, std::chrono types, and 2D/3D geometric primitives have in common?\r\n\r\nIn this talk we will present some curiously coupled data types that frequently occur in your programs, together forming notions that you are already intuitively familiar with. We will shine a light on the mathematical notion of Affine Spaces, and how they guide stronger design. We will review the properties of affine spaces and show how they improve program semantics, stronger type safety and compile time enforcement of these semantics.\r\n\r\nBy showing motivational examples, we will introduce you to the mathematical notion of affine spaces. The main focus will then be on how affine space types and their well defined semantics shape expressive APIs.\r\n\r\nWe will give examples and guidelines for creating your own affine types. Although the examples in the talk will use C++, the general concepts are applicable to other strongly typed  programming languages.\r\n"",
              ""startsAt"": ""2019-06-20T16:20:00"",
              ""endsAt"": ""2019-06-20T17:20:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""f90c9365-b46d-41ef-bfe8-5a8ce7869ab5"",
                  ""name"": ""Adi Shavit""
                },
                {
                  ""id"": ""faf284c6-cee1-4e2c-bd6c-91e92ad33400"",
                  ""name"": ""Björn Fahller""
                }
              ],
              ""categories"": [],
              ""roomId"": 4480,
              ""room"": ""Room 2""
            },
            ""index"": 1
          },
          {
            ""id"": 4481,
            ""name"": ""Room 3"",
            ""session"": {
              ""id"": ""127293"",
              ""title"": ""Anchored Metadata"",
              ""description"": ""When building software, we often need to associate metadata with the code we’re writing. A typical example is when we need to tell our linters to ignore a specific range of code. A common approach to adding this metadata is to embed it directly in the code using the syntax of the language, but this approach has a number of drawbacks including language specificity, potential for collision, and cluttering of the code.\r\n\r\nIn this talk we’ll look at an alternative approach that stores the metadata separate from the code using a technique called _anchoring_. The metadata is associated with an anchor, a region of code inside the source file. Critically, the anchor also includes a context, a snapshot of the code surrounding the anchored region. As the source code is changed, this context – along with some very interesting algorithms for aligning text -– is used to automatically update the anchors.\r\n\r\nTo demonstrate these concepts we’ll look at spor, a tool that implements anchoring and anchor updating. The primary implementation of spor is in Python, so it’s very approachable and, indeed, open for contribution. As a side note, we’ll also look at a partial implementation of spor written in Rust. Finally, we’ll look at how spor is being used in Cosmic Ray, a mutation testing tool for Python."",
              ""startsAt"": ""2019-06-20T16:20:00"",
              ""endsAt"": ""2019-06-20T17:20:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""ad6fb1a9-9b48-4393-95f1-f352e643c541"",
                  ""name"": ""Austin Bingham""
                }
              ],
              ""categories"": [],
              ""roomId"": 4481,
              ""room"": ""Room 3""
            },
            ""index"": 2
          }
        ]
      },
      {
        ""slotStart"": ""17:40:00"",
        ""rooms"": [
          {
            ""id"": 4479,
            ""name"": ""Room 1"",
            ""session"": {
              ""id"": ""84573"",
              ""title"": ""Tick Tock: What the heck is time-series data?"",
              ""description"": ""The rise of IoT and smart infrastructure has led to the generation of massive amounts of complex data. In this session, we will talk about time-series data, the challenges of working with time series data, ingestion of this data using data from NYC cabs and running real time queries to gather insights. By the end of the session, we will have an understanding of what time-series data is, how to build streaming data pipelines for massive time series data using Flink, Kafka and CrateDB, and visualising all this data with the help of a dashboard."",
              ""startsAt"": ""2019-06-20T17:40:00"",
              ""endsAt"": ""2019-06-20T18:40:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""2c535d4d-198c-4132-932c-bebe9a948306"",
                  ""name"": ""Tanay Pant""
                }
              ],
              ""categories"": [],
              ""roomId"": 4479,
              ""room"": ""Room 1""
            },
            ""index"": 0
          },
          {
            ""id"": 4480,
            ""name"": ""Room 2"",
            ""session"": {
              ""id"": ""99530"",
              ""title"": "".NET Rocks Live!"",
              ""description"": ""Migrating from WCF to .NET Core with Mark Rendle\r\nAre you looking to migrate off of WCF? Microsoft has said that WCF will not move to .NET Core – so what are your options? \r\n\r\nJoin Carl and Richard from .NET Rocks as they talk to Mark Rendle about his work building a tool to help migrate WCF applications to a combination of .NET Core, ASP.NET Web API and gRPC. Bring your questions and be part of an in-person .NET Rocks recording!"",
              ""startsAt"": ""2019-06-20T17:40:00"",
              ""endsAt"": ""2019-06-20T18:40:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""7a34a7fc-2f1c-4751-af4f-18a21de0e6b3"",
                  ""name"": ""Richard Campbell""
                },
                {
                  ""id"": ""6d1065d2-3494-48a6-a6bb-7efde3d8de40"",
                  ""name"": ""Carl Franklin""
                },
                {
                  ""id"": ""70590ad0-b1b3-40b8-b05d-b58722ef9d9d"",
                  ""name"": ""Mark Rendle""
                }
              ],
              ""categories"": [],
              ""roomId"": 4480,
              ""room"": ""Room 2""
            },
            ""index"": 1
          },
          {
            ""id"": 4481,
            ""name"": ""Room 3"",
            ""session"": {
              ""id"": ""127294"",
              ""title"": ""UX Design Fundamentals: What do your users really see"",
              ""description"": ""Developers are often unaware of how their users actually see their screens. In this UX design session, we'll discuss the most important principles concerning how the human brain and visual system determine how users see application interfaces.\r\n\r\nWe'll look at Gestalt principles for grouping and highlighting, inattentional blindness and change blindness, how users scan through a view, and how to promote clarity in interfaces with levels of emphasis. Tests will help attendees see how they personally experience these principles, and better understand the challenges faced by their users when views and pages are not designed to respect design principles."",
              ""startsAt"": ""2019-06-20T17:40:00"",
              ""endsAt"": ""2019-06-20T18:40:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""af1eed12-4400-4d1c-885f-7fbb41e737b6"",
                  ""name"": ""Billy Hollis""
                }
              ],
              ""categories"": [],
              ""roomId"": 4481,
              ""room"": ""Room 3""
            },
            ""index"": 2
          },
          {
            ""id"": 4483,
            ""name"": ""Room 5"",
            ""session"": {
              ""id"": ""97346"",
              ""title"": ""Empower Your Microservices with Istio Service Mesh"",
              ""description"": ""Microservices popularity has grown as a lot of organizations are moving their applications to microservices which enables their teams to autonomously own and operate their own microservices. The microservices have to communicate with each other so how do you efficiently connect, secure, and monitor those services? \r\n\r\nIstio is an open platform for providing a uniform way to integrate microservices, manage traffic flow across microservices, enforce policies and aggregate telemetry data.\r\n\r\nIn this session, we will cover what is service mesh and why it is important for you, what are the core components of Istio, how to empower your microservices to leverage the features that Istio provides on top of Kubernetes such as service discovery, load balancing, resiliency, observability, and security."",
              ""startsAt"": ""2019-06-20T17:40:00"",
              ""endsAt"": ""2019-06-20T18:40:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""80f14329-6da2-4fb3-915d-7a2b1f49619f"",
                  ""name"": ""Hossam Barakat""
                }
              ],
              ""categories"": [],
              ""roomId"": 4483,
              ""room"": ""Room 5""
            },
            ""index"": 4
          },
          {
            ""id"": 4484,
            ""name"": ""Room 6"",
            ""session"": {
              ""id"": ""76044"",
              ""title"": ""Advanced .NET debugging techniques from real world investigations"",
              ""description"": ""You know how it feels. After releasing a new version, a service starts behaving in an unexpected way, and it's up to you to save the day. But where to start?\r\n\r\nCriteo processes 150 billion requests per day, across more than 4000 front-end servers. As part of the Criteo Performance team, our job is to investigate critical issues in this kind of environment.\r\n\r\nIn this talk, you will follow our insights, mistakes and false leads during a real world case.\r\n\r\nWe will cover all the phases of the investigation, from the early detection to the actual fix, and we will detail our tricks and tools along the way. Including but not limited to:\r\n\r\n - Using metrics to detect and assess the issue;\r\n - What you can get... or not from a profiler to make a good assumption;\r\n - Digging into the CLR data structures with a decompiler, WinDBG and SOS to assert your assumption;\r\n - Automating memory dump analysis with ClrMD to build your own tools when WinDBG falls short."",
              ""startsAt"": ""2019-06-20T17:40:00"",
              ""endsAt"": ""2019-06-20T18:40:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""a147c43c-8537-4cc7-92f5-41ee2c1d67ab"",
                  ""name"": ""Kevin Gosse""
                },
                {
                  ""id"": ""62706aaf-aadc-4538-9f39-70f5afb4cf0b"",
                  ""name"": ""Christophe Nasarre""
                }
              ],
              ""categories"": [],
              ""roomId"": 4484,
              ""room"": ""Room 6""
            },
            ""index"": 5
          },
          {
            ""id"": 4485,
            ""name"": ""Room 7"",
            ""session"": {
              ""id"": ""98882"",
              ""title"": ""What vulnerabilities?  Live hacking of containers and orchestrators"",
              ""description"": ""We often see alerts about vulnerabilities being found in frameworks that we use today, but should we really care about them?  What's the worst that can happen?  Can someone own a container?  Could they run a bitcoin miner on my servers?  Are they able to own the cluster?\r\n\r\nIn this talk, we look at one of the worst-case scenarios from a real-world perspective.  We have a red team member attempting to hack a cluster we own with a live hack on stage whilst the blue team member tries to stop it from happening.\r\n\r\nWe'll discuss developing best practices, implement security policies and how best to monitor your services to put preventative measures in place.\r\n\r\n"",
              ""startsAt"": ""2019-06-20T17:40:00"",
              ""endsAt"": ""2019-06-20T18:40:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""964af3ac-fd5e-46f6-b582-6c0d2da30db4"",
                  ""name"": ""Lewis Denham-Parry""
                }
              ],
              ""categories"": [],
              ""roomId"": 4485,
              ""room"": ""Room 7""
            },
            ""index"": 6
          },
          {
            ""id"": 4486,
            ""name"": ""Room 8"",
            ""session"": {
              ""id"": ""74878"",
              ""title"": ""Powering 100+ million daily users"",
              ""description"": ""In this talk I will walk through how we build a simple architecture for a complex system with strict performance (250 ms) and scale (100+ million connected daily users) requirements in order to power people experiences in Outlook, Owa, OneDrive, etc.  Also share our thinking of how to slowly and gradually shift the engineering mindset to embrace microservices without scratching everything and starting from scratch."",
              ""startsAt"": ""2019-06-20T17:40:00"",
              ""endsAt"": ""2019-06-20T18:40:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""8d5c7076-3579-40c3-9fbf-a02749078257"",
                  ""name"": ""Rezaul Hoque""
                }
              ],
              ""categories"": [],
              ""roomId"": 4486,
              ""room"": ""Room 8""
            },
            ""index"": 7
          },
          {
            ""id"": 4487,
            ""name"": ""Room 9"",
            ""session"": {
              ""id"": ""98723"",
              ""title"": ""5 Tips for Cultivating EQ in the Workplace"",
              ""description"": ""Learning to manage our state of mind in the workplace is an acquired skill. While stress in the workplace in unavoidable, it is possible to cultivate Emotional Intelligence (EQ) to manage our state of mind. Practicing EQ helps us identify and eliminate stressors in our lives. Awareness of self and awareness of others strengthens personal and professional relationships. When we understand the motivations of ourselves and the perspectives of others we form deeper connections. In this presentation, learn five tips for cultivating Emotional Intelligence in the workplace.\r\n\r\n"",
              ""startsAt"": ""2019-06-20T17:40:00"",
              ""endsAt"": ""2019-06-20T18:40:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""e296f852-2d30-4f51-8cf6-ac3c87b1427e"",
                  ""name"": ""Christina Aldan""
                }
              ],
              ""categories"": [],
              ""roomId"": 4487,
              ""room"": ""Room 9""
            },
            ""index"": 8
          },
          {
            ""id"": 4488,
            ""name"": ""Room 10"",
            ""session"": {
              ""id"": ""128716"",
              ""title"": ""Workshop: DIY – build your own React - Part 2/2"",
              ""description"": ""Are you using React, but don’t really know how it works? The framework is quite simple and do not require understanding the underlying mechanisms for most day to day tasks. This workshop will take your skills up a level, giving you a thorough understanding of React.\r\n\r\nYou will create a working version of React in less than 200 lines of code. It won’t be as efficient as React is. Nonetheless, you will gain valuable insight into how React work under the hood. We will demystify a lot of the concepts that are taken for granted when using React: representing the DOM-tree, rendering components, setting state and props, and re-rendering."",
              ""startsAt"": ""2019-06-20T17:40:00"",
              ""endsAt"": ""2019-06-20T18:40:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""e91a37c2-fe9c-46f2-bef4-2e67e2e114a9"",
                  ""name"": ""Henrik Hermansen""
                },
                {
                  ""id"": ""a9b0694d-8478-4b05-b368-e89d138323f8"",
                  ""name"": ""Svein Petter Gjøby""
                },
                {
                  ""id"": ""79cb3124-4d83-4b49-adcd-9035a0c45135"",
                  ""name"": ""Eirik Vigeland""
                }
              ],
              ""categories"": [],
              ""roomId"": 4488,
              ""room"": ""Room 10""
            },
            ""index"": 9
          }
        ]
      },
      {
        ""slotStart"": ""18:40:00"",
        ""rooms"": [
          {
            ""id"": 4489,
            ""name"": ""Expo"",
            ""session"": {
              ""id"": ""128960"",
              ""title"": ""Party"",
              ""description"": ""Kick back, get to know your fellow attendees and enjoy live music from the main stage. The party is complimentary for all NDC delegates. (18:40- 23:00)\r\n\r\nSchedule:\r\n\r\n- Conference Reception in the Expo\r\n- Fun talks with host Lars Klint\r\n- Dylan Beattie and the Linebreakers\r\n- LoveShack"",
              ""startsAt"": ""2019-06-20T18:40:00"",
              ""endsAt"": ""2019-06-20T23:00:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""f99fa878-6176-4ff4-b151-c717fa5daf0c"",
                  ""name"": ""Lars Klint""
                },
                {
                  ""id"": ""1d7dcbfc-1de6-4228-8bd6-04f4ba1c4267"",
                  ""name"": ""Dylan Beattie""
                }
              ],
              ""categories"": [],
              ""roomId"": 4489,
              ""room"": ""Expo""
            },
            ""index"": 11
          }
        ]
      }
    ]
  },
  {
    ""date"": ""2019-06-21T00:00:00"",
    ""rooms"": [
      {
        ""id"": 4479,
        ""name"": ""Room 1"",
        ""sessions"": [
          {
            ""id"": ""81395"",
            ""title"": ""Machine Learning: The Bare Math Behind Libraries"",
            ""description"": ""Machine learning is one of the hottest buzzwords in technology today as well as one of the most innovative fields in computer science – yet people use libraries as black boxes without basic knowledge of the field. In this session, we will strip them to bare math, so next time you use a machine learning library, you'll have a deeper understanding of what lies underneath.\r\n\r\nDuring this session, we will first provide a short history of machine learning and an overview of two basic teaching techniques: supervised and unsupervised learning.\r\n\r\nWe will start by defining what machine learning is and equip you with an intuition of how it works. We will then explain gradient descent algorithm with the use of simple linear regression to give you an even deeper understanding of this learning method. Then we will project it to supervised neural networks training.\r\n\r\nWithin unsupervised learning, you will become familiar with Hebb’s learning and learning with concurrency (winner takes all and winner takes most algorithms). We will use Octave for examples in this session; however, you can use your favorite technology to implement presented ideas.\r\n\r\nOur aim is to show the mathematical basics of neural networks for those who want to start using machine learning in their day-to-day work or use it already but find it difficult to understand the underlying processes. After viewing our presentation, you should find it easier to select parameters for your networks and feel more confident in your selection of network type, as well as be encouraged to dive into more complex and powerful deep learning methods.\r\n\r\nLevel: beginner\r\n"",
            ""startsAt"": ""2019-06-21T10:20:00"",
            ""endsAt"": ""2019-06-21T11:20:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""f5715729-0aeb-4945-a65e-9dd0d6ddf424"",
                ""name"": ""Łukasz Gebel""
              },
              {
                ""id"": ""826496af-d697-40eb-a5d8-2c16bda34181"",
                ""name"": ""Piotr Czajka""
              }
            ],
            ""categories"": [],
            ""roomId"": 4479,
            ""room"": ""Room 1""
          },
          {
            ""id"": ""84059"",
            ""title"": ""Protecting sensitive data in huge datasets: Cloud tools you can use"",
            ""description"": ""Before releasing a public dataset, practitioners need to thread the needle between utility and protection of individuals. Felipe Hoffa explores how to handle massive public datasets, taking you from theory to real life as they showcase newly available tools that help with PII detection and brings concepts like k-anonymity and l-diversity to the practical realm. You’ll also cover options such as removing, masking, and coarsening.\r\n\r\nWhat you'll learn:\r\n\r\n- Learn how to identify PII in massive datasets\r\n- Explore k-anonymity, l-diversity, and related research and options such as removing, masking, and coarsening\r\n- Gain experience with practical demos over massive datasets"",
            ""startsAt"": ""2019-06-21T11:40:00"",
            ""endsAt"": ""2019-06-21T12:40:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""28dd5c97-7753-4acf-b956-25e7467c31ab"",
                ""name"": ""Felipe Hoffa""
              }
            ],
            ""categories"": [],
            ""roomId"": 4479,
            ""room"": ""Room 1""
          },
          {
            ""id"": ""84838"",
            ""title"": ""ML BuzzWords demystified"",
            ""description"": ""Machine Learning is a fast evolving discipline and one of the hottest areas both in industry and academia, and it only keeps getting more traction. With such a quickly advancing field, it becomes increasingly hard to keep up with the new concepts. \r\nIf you find yourself lost in a forest of ML buzzwords and want to catch up, welcome to our session!\r\nWe will give you the gist of the latest trends in Machine Learning - from Reinforcement Learning and AutoML to ML bias - with zero formulas and maximum sense.\r\n\r\nBy the end of the session, you will be up-to-date with what is happening in the field."",
            ""startsAt"": ""2019-06-21T13:40:00"",
            ""endsAt"": ""2019-06-21T14:40:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""61a48ebe-ebe0-427f-a1a1-eef2f7e92ed9"",
                ""name"": ""Oleksandra Sopova""
              },
              {
                ""id"": ""5937af3f-6906-46e8-86a9-0e9cefd5a080"",
                ""name"": ""Natalia An""
              }
            ],
            ""categories"": [],
            ""roomId"": 4479,
            ""room"": ""Room 1""
          },
          {
            ""id"": ""98658"",
            ""title"": ""The Hitchhiker's Guide to the Cloud (AWS vs GCP vs Azure) and their AI/ML API’s capabilities"",
            ""description"": ""To companies leveraging the capabilities of public cloud (often Amazon Web Services, Google Cloud or Microsoft Azure) the felling of immersion into a single provider platform is constant in their day to day. With a rapid evolution of services becoming available in each cloud provider, companies tend to focus and keep updated with only one of them while other providers capabilities are simply unknown, ignored or forgotten. \r\nOn the other hand, there are many companies that are not yet using public cloud and are now facing the dilemma of which Public Cloud provider to choose.\r\n\r\nAI and Machine Learning are key areas of investment, growth and differentiation for many companies and that is no exception for the three biggest public cloud players (AWS, GCP and Azure). In this context, pre-trained AI/ML API’s in combination with other Serverless services is one area that has been on the rise and with fast adoption. \r\n\r\nIn this talk we will learn about the three major public cloud providers (AWS, GCP and Azure) by having an overview and gain insights about each other pros and cons. In addition, we are going to explore their AI/ML Cloud API’s that allow us to leverage ready-made capabilities such as: Text to Speech, Image & Video Classification, Translation,  Speech Recognition, Sentiment Analysis, etc.\r\n\r\n"",
            ""startsAt"": ""2019-06-21T15:00:00"",
            ""endsAt"": ""2019-06-21T16:00:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""a2e6985d-7ead-4b43-bdde-bfdea9923300"",
                ""name"": ""Bruno Amaro Almeida""
              }
            ],
            ""categories"": [],
            ""roomId"": 4479,
            ""room"": ""Room 1""
          },
          {
            ""id"": ""99155"",
            ""title"": ""Everything is Cyber-broken 2"",
            ""description"": ""TBA - submitting this now so you have it in the agenda, it'll be an all new talk in the theme of the first cyber-broken talk"",
            ""startsAt"": ""2019-06-21T16:20:00"",
            ""endsAt"": ""2019-06-21T17:20:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""735a4b60-42e8-4452-9480-68197372c206"",
                ""name"": ""Troy Hunt""
              }
            ],
            ""categories"": [],
            ""roomId"": 4479,
            ""room"": ""Room 1""
          }
        ],
        ""hasOnlyPlenumSessions"": false
      },
      {
        ""id"": 4480,
        ""name"": ""Room 2"",
        ""sessions"": [
          {
            ""id"": ""98412"",
            ""title"": ""Hacking with Go"",
            ""description"": ""Learning Go programming is easy. Go is popular and becomes even more also in security experts world. Wanted to feel a bit as a hacker? Learn a new language? Or do both at the same time? This session is about it. \r\nSo let's jump into hands-on session and explore how security tools can be written in Go. How to enumerate network resources, extract an information, sniff packets and do port scanning, brute force and more all with Go. \r\nBy the end, you will have more ideas what else can be written or re-written in Go. "",
            ""startsAt"": ""2019-06-21T10:20:00"",
            ""endsAt"": ""2019-06-21T11:20:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""4c3efe89-ab58-4b28-a9c3-78251f25ee06"",
                ""name"": ""Victoria Almazova""
              }
            ],
            ""categories"": [],
            ""roomId"": 4480,
            ""room"": ""Room 2""
          },
          {
            ""id"": ""98825"",
            ""title"": ""Source Instrumentation for Monitoring C++ in Production"",
            ""description"": ""It is essential to discuss how modern C++ code can be effectively instrumented, in order to effectively monitor it after deployment. This talk will focus on portable source instrumentation techniques such as logging, tracing and metrics. Straightforward, but well designed code additions can drastically ease the troubleshooting of functional issues, and identification of performance bottlenecks, in production.\r\n\r\nOf course when dealing with C++ performance is often critical, and so minimizing the cost of any instrumentation is also critical. Key to this is understanding the trade-off between the detail of information collected, and the overheads of exposing that information. It is also important to understand how best to benefit from advances in contemporary monitoring infrastructure, popularised by cloud environments.\r\n\r\nThis talk will open with some brief motivation towards monitoring and instrumentation. It will then walk through some practical code examples using some generic instrumentation primitives, based on proven principles employed in demanding production software. "",
            ""startsAt"": ""2019-06-21T11:40:00"",
            ""endsAt"": ""2019-06-21T12:40:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""8ccea10a-7d68-4c6a-9d59-88f9355ccfff"",
                ""name"": ""Steven Simpson""
              }
            ],
            ""categories"": [],
            ""roomId"": 4480,
            ""room"": ""Room 2""
          },
          {
            ""id"": ""99876"",
            ""title"": ""Lightning Talks (Diverse)"",
            ""description"": ""Lightning talks (approx 10-15 minutes each)\r\n\r\nTalk 1: 5 Lessons Learned from Implementing 40+ Machine Learning Projects - Xiaopeng Li\r\n\r\nMachine learning has reached the peak of Gartner's hype curve in 2018. Many companies are talking about it, while very few are actually doing it. At Inmeta, we have together with partners and clients implemented 40+ machine learning projects in the past few years. In this talk, I will share the top 5 lessons we learned from doing machine learning for real.\r\n\r\n-----------------------------------------------------------------------\r\n\r\nTalk 2: test && commit || revert. What?! - Kari Eline Strandjord\r\n\r\n`test && commit || revert` is a workflow developed during a week of code camp at Iterate with Kent Beck. The main idea is that your code should always be in a valid state. If the test passes, the code is committed. If it fails, you lose all your changes, and the code is forced back to the last valid state. At first, the idea seemed both unrealistic and a bit harsh. But after trying it out during a longer period writing an application in Elm, it did change my way of coding. \r\n\r\nThe talk will give a brief explanation of the workflow. I will also share my experiences from using it in a real project.\r\n\r\n-----------------------------------------------------------------------\r\n\r\nTalk 3: Five Ways to Break a Git Repository - Edward Thomson\r\n\r\nCan you break your Git repository? I hope not! That's where you keep all your stuff! Edward Thomson shows you five common mistakes that break Git repositories and how to fix them.\r\n\r\n-----------------------------------------------------------------------\r\n\r\nTalk 4: Unlocking the doors of parliament - Sindre Lindstad\r\n\r\nWhen Norway's Minister of Children and Families was instated, he enthusiastically showed the world the key to his new office through press photos.\r\n\r\nThe only problem was that it was a plastic punch-hole keycard, which meant anyone could make a copy.\r\n\r\nSo I made one with 3D printing (and lasers!). But does it work?\r\n\r\n----------------------------------------------------------------------- "",
            ""startsAt"": ""2019-06-21T13:40:00"",
            ""endsAt"": ""2019-06-21T14:40:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""99aa52b8-e0ab-4bd8-be19-437cc4b93740"",
                ""name"": ""Xiaopeng Li""
              },
              {
                ""id"": ""a5357148-b11b-4ab0-822c-b650881773ef"",
                ""name"": ""Kari Eline Strandjord""
              },
              {
                ""id"": ""fee375b8-047c-4ea2-ab43-9837f2420b19"",
                ""name"": ""Edward Thomson""
              },
              {
                ""id"": ""5ec0ca76-bf24-4fce-bf3d-80656dadf1f6"",
                ""name"": ""Sindre Lindstad""
              }
            ],
            ""categories"": [],
            ""roomId"": 4480,
            ""room"": ""Room 2""
          },
          {
            ""id"": ""99320"",
            ""title"": ""Trying to learn C#"",
            ""description"": ""Learning a new language is often colored by the language you come from. As a programmer coming from C++ and Java, with some functional programming background, how did I navigate trying to get a grasp of C#? Should be fun for C# developers, but also educational: How do we teach a new language to folks that already know how to program?"",
            ""startsAt"": ""2019-06-21T15:00:00"",
            ""endsAt"": ""2019-06-21T16:00:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""0eaa4bb2-cb2a-4b76-800d-de8b1dfdb50c"",
                ""name"": ""Patricia Aas""
              }
            ],
            ""categories"": [],
            ""roomId"": 4480,
            ""room"": ""Room 2""
          },
          {
            ""id"": ""86521"",
            ""title"": ""Rediscovering fire - on designing portable, multi-language libraries"",
            ""description"": ""The session will cover the design choices and lessons learned developing the multi-language free library segyio, or more conceptually, designing libraries for libraries.\r\n\r\nBriefly, it will discuss:\r\n- Stable API, ABI, and how to design them for the future\r\n- How to design C-interface libraries that allows for good foreign-language libraries (in our case python)\r\n- Library design philosophy and the beauty of primitive functions\r\n- How to design for composition and caller flexibility\r\n- Plumbing and porcelain\r\n\r\nThe session should appeal both to library developers for embedded systems, and consumers of higher-level libraries in desktop and scientific applications, as the topic covered is the bridge between primitive and sophisticated systems, and making it beautiful."",
            ""startsAt"": ""2019-06-21T16:20:00"",
            ""endsAt"": ""2019-06-21T17:20:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""142b682d-b548-423c-8e51-e36ee08db70f"",
                ""name"": ""Jørgen Kvalsvik""
              }
            ],
            ""categories"": [],
            ""roomId"": 4480,
            ""room"": ""Room 2""
          }
        ],
        ""hasOnlyPlenumSessions"": false
      },
      {
        ""id"": 4481,
        ""name"": ""Room 3"",
        ""sessions"": [
          {
            ""id"": ""98167"",
            ""title"": ""Evolving compositional user interfaces"",
            ""description"": ""Ever since we started breaking applications into services, be it in the era of SOA or more recently with microservices, we’ve struggled to incorporate user interfaces into our decoupled, distributed architectures. We’ve seen frontends versioned separately with tight coupling to our services, breaking cohesion. We’ve seen the rise of Backend-For-Frontend and the emerge of micro frontends. We talk about composition, yet so many projects fail to implement actual composition. Instead we end up with some kind of compromise, with repeated business logic in the front-end, back-end and API, making it hard to scale – especially when multiple teams are involved – causing lock-step deployment, latency, bottlenecks and coordination issues.\r\n\r\nWhat if we could find a viable solution that allowed us to scale development, keep distribution and cohesion and also provide composition of user interfaces?\r\n\r\nIn this talk you are introduced to the evolution of compositional user interfaces and existing patterns while we discover their pros and cons, before diving into the architecture and development of compositional interfaces using hypermedia and micro-frontends. We go beyond the simple “Hello World” example that always seems to work, and you’ll learn patterns in modelling and design that will get you up and running with decoupled, composed user interfaces in your day job.\r\n"",
            ""startsAt"": ""2019-06-21T10:20:00"",
            ""endsAt"": ""2019-06-21T11:20:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""a6b5a582-ff8e-4874-84c7-35e095954666"",
                ""name"": ""Thomas Presthus""
              },
              {
                ""id"": ""1e4b0b48-cc95-4059-bff4-32103fb3a496"",
                ""name"": ""Asbjørn Ulsberg""
              }
            ],
            ""categories"": [],
            ""roomId"": 4481,
            ""room"": ""Room 3""
          },
          {
            ""id"": ""128875"",
            ""title"": ""Fabulous – F# for cross-platform mobile apps"",
            ""description"": ""In this informal talk, I will describe Fabulous, a community-developed framework using F# to build cross-platform mobile and desktop Xamarin apps.\r\n\r\nCome and learn how this radical new approach to app programming makes your code simpler, more testable and helps avoid repetition. By embracing the React-like MVU architecture, you can do away with your Xaml, your behaviours, your converters, your templating, your MVVM and embrace the simplicity of functional model descriptions and view re-evalaution. I will talk about the concepts involved and how this differs from Model-View-ViewModel (MVVM), the tooling available and how yuo can get involved.\r\n\r\nThis is mostly a conceptual talk and won’t be full of sparkling demos: demo apps are available from the Fabulous community.\r\n\r\nNote, Fabulous is a community project and is at version 0.34 as of May 2019. It is not a supported product or framework from Microsoft.\r\n\r\nhttps://fsprojects.github.io/Fabulous/"",
            ""startsAt"": ""2019-06-21T11:40:00"",
            ""endsAt"": ""2019-06-21T12:40:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""68b8aef8-e1b3-4a1f-9b73-2562ec1af738"",
                ""name"": ""Don Syme""
              }
            ],
            ""categories"": [],
            ""roomId"": 4481,
            ""room"": ""Room 3""
          },
          {
            ""id"": ""98611"",
            ""title"": ""An in-flight port from Angular to React, a tale of performance and happiness"",
            ""description"": ""A real-world story of how we used some clever trickery to completely rewrite an app, bit by bit from Angular to React, resulting in better performance, a smaller footprint, a shorter feedback loop, less coupling, fewer bugs, increased development velocity and happier developers.\r\nWe did the re-write, while deploying to production frequently. New features were added to the product throughout the whole rewrite process, and stability was maintained throughout the entire process.\r\n\r\nThe application is an e-commerce payment solution (Nets Easy), used by numerous merchants in their web shops to get paid."",
            ""startsAt"": ""2019-06-21T13:40:00"",
            ""endsAt"": ""2019-06-21T14:40:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""912bbb93-e7bd-4448-8854-19ef39fa5843"",
                ""name"": ""Henning Christiansen""
              },
              {
                ""id"": ""1e49e725-0923-4dae-b00d-f443c518b010"",
                ""name"": ""Francis Paulin""
              }
            ],
            ""categories"": [],
            ""roomId"": 4481,
            ""room"": ""Room 3""
          },
          {
            ""id"": ""99143"",
            ""title"": ""It's about time"",
            ""description"": ""Time Zones, Daylight savings, Leap years, Leap seconds... Storing it all, testing it, getting it right for every point in time in every country... \r\nWriting correct timing code can be a nightmare! \r\nWe'll be ranting our way through some common pitfalls, tips and tricks to enable you to reason more effectively about time in your applications."",
            ""startsAt"": ""2019-06-21T15:00:00"",
            ""endsAt"": ""2019-06-21T16:00:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""d28ae055-13a5-4d0a-8668-24fd93198cef"",
                ""name"": ""Christin Gorman""
              }
            ],
            ""categories"": [],
            ""roomId"": 4481,
            ""room"": ""Room 3""
          },
          {
            ""id"": ""99253"",
            ""title"": ""Who's Who? Federating Identity with Azure B2C"",
            ""description"": ""Often, users of your system will already know who they are, or at least think they do. Making sure you know who they are and what they can do is pretty important too.\r\n\r\nIn this session Microsoft Engineer Andrew Coates will present techniques for allowing users to log into your system with credentials from another system. Using Azure B2C allows you to offload authentication to other identity providers while keeping authorization tasks local to your system.\r\n\r\nOffload the hassles of lost passwords, expiring accounts and more, leaving you time to build and maintain the things that are important to your system.\r\n\r\nAndrew will demonstrate the setup and configuration of this powerful identity federation system allowing integration of any combination of social identities such as Facebook or twitter, as well as organisational accounts like Active Directory and others. He'll also discuss the extension points allowing complete control of the identity system including rules-based identity flows and calling out to custom REST services as part of the claims processing flow,\r\n\r\nIf your system needs to include users from outside your organisation, this is a must-see session.\r\n"",
            ""startsAt"": ""2019-06-21T16:20:00"",
            ""endsAt"": ""2019-06-21T17:20:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""2809c750-dc83-4e1c-8f06-38ee96b818b6"",
                ""name"": ""Andrew Coates""
              }
            ],
            ""categories"": [],
            ""roomId"": 4481,
            ""room"": ""Room 3""
          }
        ],
        ""hasOnlyPlenumSessions"": false
      },
      {
        ""id"": 4489,
        ""name"": ""Expo"",
        ""sessions"": [
          {
            ""id"": ""99510"",
            ""title"": ""Enterprise transformation (and you can too)"",
            ""description"": ""“That would never work here.” You’ve likely heard this sentiment (or maybe you’ve even said it yourself). Good news: change is possible. Donovan Brown explains how Microsoft's Azure DevOps formerly VSTS went from a three-year waterfall delivery cycle to three-week iterations and open sourced the Azure DevOps task library and the Git Virtual File System."",
            ""startsAt"": ""2019-06-21T09:00:00"",
            ""endsAt"": ""2019-06-21T10:00:00"",
            ""isServiceSession"": false,
            ""isPlenumSession"": false,
            ""speakers"": [
              {
                ""id"": ""266bf958-048e-4c55-b01f-398b90dfe5e9"",
                ""name"": ""Donovan Brown""
              }
            ],
            ""categories"": [],
            ""roomId"": 4489,
            ""room"": ""Expo""
          }
        ],
        ""hasOnlyPlenumSessions"": false
      }
    ],
    ""timeSlots"": [
      {
        ""slotStart"": ""09:00:00"",
        ""rooms"": [
          {
            ""id"": 4489,
            ""name"": ""Expo"",
            ""session"": {
              ""id"": ""99510"",
              ""title"": ""Enterprise transformation (and you can too)"",
              ""description"": ""“That would never work here.” You’ve likely heard this sentiment (or maybe you’ve even said it yourself). Good news: change is possible. Donovan Brown explains how Microsoft's Azure DevOps formerly VSTS went from a three-year waterfall delivery cycle to three-week iterations and open sourced the Azure DevOps task library and the Git Virtual File System."",
              ""startsAt"": ""2019-06-21T09:00:00"",
              ""endsAt"": ""2019-06-21T10:00:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""266bf958-048e-4c55-b01f-398b90dfe5e9"",
                  ""name"": ""Donovan Brown""
                }
              ],
              ""categories"": [],
              ""roomId"": 4489,
              ""room"": ""Expo""
            },
            ""index"": 11
          }
        ]
      },
      {
        ""slotStart"": ""10:20:00"",
        ""rooms"": [
          {
            ""id"": 4479,
            ""name"": ""Room 1"",
            ""session"": {
              ""id"": ""81395"",
              ""title"": ""Machine Learning: The Bare Math Behind Libraries"",
              ""description"": ""Machine learning is one of the hottest buzzwords in technology today as well as one of the most innovative fields in computer science – yet people use libraries as black boxes without basic knowledge of the field. In this session, we will strip them to bare math, so next time you use a machine learning library, you'll have a deeper understanding of what lies underneath.\r\n\r\nDuring this session, we will first provide a short history of machine learning and an overview of two basic teaching techniques: supervised and unsupervised learning.\r\n\r\nWe will start by defining what machine learning is and equip you with an intuition of how it works. We will then explain gradient descent algorithm with the use of simple linear regression to give you an even deeper understanding of this learning method. Then we will project it to supervised neural networks training.\r\n\r\nWithin unsupervised learning, you will become familiar with Hebb’s learning and learning with concurrency (winner takes all and winner takes most algorithms). We will use Octave for examples in this session; however, you can use your favorite technology to implement presented ideas.\r\n\r\nOur aim is to show the mathematical basics of neural networks for those who want to start using machine learning in their day-to-day work or use it already but find it difficult to understand the underlying processes. After viewing our presentation, you should find it easier to select parameters for your networks and feel more confident in your selection of network type, as well as be encouraged to dive into more complex and powerful deep learning methods.\r\n\r\nLevel: beginner\r\n"",
              ""startsAt"": ""2019-06-21T10:20:00"",
              ""endsAt"": ""2019-06-21T11:20:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""f5715729-0aeb-4945-a65e-9dd0d6ddf424"",
                  ""name"": ""Łukasz Gebel""
                },
                {
                  ""id"": ""826496af-d697-40eb-a5d8-2c16bda34181"",
                  ""name"": ""Piotr Czajka""
                }
              ],
              ""categories"": [],
              ""roomId"": 4479,
              ""room"": ""Room 1""
            },
            ""index"": 0
          },
          {
            ""id"": 4480,
            ""name"": ""Room 2"",
            ""session"": {
              ""id"": ""98412"",
              ""title"": ""Hacking with Go"",
              ""description"": ""Learning Go programming is easy. Go is popular and becomes even more also in security experts world. Wanted to feel a bit as a hacker? Learn a new language? Or do both at the same time? This session is about it. \r\nSo let's jump into hands-on session and explore how security tools can be written in Go. How to enumerate network resources, extract an information, sniff packets and do port scanning, brute force and more all with Go. \r\nBy the end, you will have more ideas what else can be written or re-written in Go. "",
              ""startsAt"": ""2019-06-21T10:20:00"",
              ""endsAt"": ""2019-06-21T11:20:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""4c3efe89-ab58-4b28-a9c3-78251f25ee06"",
                  ""name"": ""Victoria Almazova""
                }
              ],
              ""categories"": [],
              ""roomId"": 4480,
              ""room"": ""Room 2""
            },
            ""index"": 1
          },
          {
            ""id"": 4481,
            ""name"": ""Room 3"",
            ""session"": {
              ""id"": ""98167"",
              ""title"": ""Evolving compositional user interfaces"",
              ""description"": ""Ever since we started breaking applications into services, be it in the era of SOA or more recently with microservices, we’ve struggled to incorporate user interfaces into our decoupled, distributed architectures. We’ve seen frontends versioned separately with tight coupling to our services, breaking cohesion. We’ve seen the rise of Backend-For-Frontend and the emerge of micro frontends. We talk about composition, yet so many projects fail to implement actual composition. Instead we end up with some kind of compromise, with repeated business logic in the front-end, back-end and API, making it hard to scale – especially when multiple teams are involved – causing lock-step deployment, latency, bottlenecks and coordination issues.\r\n\r\nWhat if we could find a viable solution that allowed us to scale development, keep distribution and cohesion and also provide composition of user interfaces?\r\n\r\nIn this talk you are introduced to the evolution of compositional user interfaces and existing patterns while we discover their pros and cons, before diving into the architecture and development of compositional interfaces using hypermedia and micro-frontends. We go beyond the simple “Hello World” example that always seems to work, and you’ll learn patterns in modelling and design that will get you up and running with decoupled, composed user interfaces in your day job.\r\n"",
              ""startsAt"": ""2019-06-21T10:20:00"",
              ""endsAt"": ""2019-06-21T11:20:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""a6b5a582-ff8e-4874-84c7-35e095954666"",
                  ""name"": ""Thomas Presthus""
                },
                {
                  ""id"": ""1e4b0b48-cc95-4059-bff4-32103fb3a496"",
                  ""name"": ""Asbjørn Ulsberg""
                }
              ],
              ""categories"": [],
              ""roomId"": 4481,
              ""room"": ""Room 3""
            },
            ""index"": 2
          }
        ]
      },
      {
        ""slotStart"": ""11:40:00"",
        ""rooms"": [
          {
            ""id"": 4479,
            ""name"": ""Room 1"",
            ""session"": {
              ""id"": ""84059"",
              ""title"": ""Protecting sensitive data in huge datasets: Cloud tools you can use"",
              ""description"": ""Before releasing a public dataset, practitioners need to thread the needle between utility and protection of individuals. Felipe Hoffa explores how to handle massive public datasets, taking you from theory to real life as they showcase newly available tools that help with PII detection and brings concepts like k-anonymity and l-diversity to the practical realm. You’ll also cover options such as removing, masking, and coarsening.\r\n\r\nWhat you'll learn:\r\n\r\n- Learn how to identify PII in massive datasets\r\n- Explore k-anonymity, l-diversity, and related research and options such as removing, masking, and coarsening\r\n- Gain experience with practical demos over massive datasets"",
              ""startsAt"": ""2019-06-21T11:40:00"",
              ""endsAt"": ""2019-06-21T12:40:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""28dd5c97-7753-4acf-b956-25e7467c31ab"",
                  ""name"": ""Felipe Hoffa""
                }
              ],
              ""categories"": [],
              ""roomId"": 4479,
              ""room"": ""Room 1""
            },
            ""index"": 0
          },
          {
            ""id"": 4480,
            ""name"": ""Room 2"",
            ""session"": {
              ""id"": ""98825"",
              ""title"": ""Source Instrumentation for Monitoring C++ in Production"",
              ""description"": ""It is essential to discuss how modern C++ code can be effectively instrumented, in order to effectively monitor it after deployment. This talk will focus on portable source instrumentation techniques such as logging, tracing and metrics. Straightforward, but well designed code additions can drastically ease the troubleshooting of functional issues, and identification of performance bottlenecks, in production.\r\n\r\nOf course when dealing with C++ performance is often critical, and so minimizing the cost of any instrumentation is also critical. Key to this is understanding the trade-off between the detail of information collected, and the overheads of exposing that information. It is also important to understand how best to benefit from advances in contemporary monitoring infrastructure, popularised by cloud environments.\r\n\r\nThis talk will open with some brief motivation towards monitoring and instrumentation. It will then walk through some practical code examples using some generic instrumentation primitives, based on proven principles employed in demanding production software. "",
              ""startsAt"": ""2019-06-21T11:40:00"",
              ""endsAt"": ""2019-06-21T12:40:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""8ccea10a-7d68-4c6a-9d59-88f9355ccfff"",
                  ""name"": ""Steven Simpson""
                }
              ],
              ""categories"": [],
              ""roomId"": 4480,
              ""room"": ""Room 2""
            },
            ""index"": 1
          },
          {
            ""id"": 4481,
            ""name"": ""Room 3"",
            ""session"": {
              ""id"": ""128875"",
              ""title"": ""Fabulous – F# for cross-platform mobile apps"",
              ""description"": ""In this informal talk, I will describe Fabulous, a community-developed framework using F# to build cross-platform mobile and desktop Xamarin apps.\r\n\r\nCome and learn how this radical new approach to app programming makes your code simpler, more testable and helps avoid repetition. By embracing the React-like MVU architecture, you can do away with your Xaml, your behaviours, your converters, your templating, your MVVM and embrace the simplicity of functional model descriptions and view re-evalaution. I will talk about the concepts involved and how this differs from Model-View-ViewModel (MVVM), the tooling available and how yuo can get involved.\r\n\r\nThis is mostly a conceptual talk and won’t be full of sparkling demos: demo apps are available from the Fabulous community.\r\n\r\nNote, Fabulous is a community project and is at version 0.34 as of May 2019. It is not a supported product or framework from Microsoft.\r\n\r\nhttps://fsprojects.github.io/Fabulous/"",
              ""startsAt"": ""2019-06-21T11:40:00"",
              ""endsAt"": ""2019-06-21T12:40:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""68b8aef8-e1b3-4a1f-9b73-2562ec1af738"",
                  ""name"": ""Don Syme""
                }
              ],
              ""categories"": [],
              ""roomId"": 4481,
              ""room"": ""Room 3""
            },
            ""index"": 2
          }
        ]
      },
      {
        ""slotStart"": ""13:40:00"",
        ""rooms"": [
          {
            ""id"": 4479,
            ""name"": ""Room 1"",
            ""session"": {
              ""id"": ""84838"",
              ""title"": ""ML BuzzWords demystified"",
              ""description"": ""Machine Learning is a fast evolving discipline and one of the hottest areas both in industry and academia, and it only keeps getting more traction. With such a quickly advancing field, it becomes increasingly hard to keep up with the new concepts. \r\nIf you find yourself lost in a forest of ML buzzwords and want to catch up, welcome to our session!\r\nWe will give you the gist of the latest trends in Machine Learning - from Reinforcement Learning and AutoML to ML bias - with zero formulas and maximum sense.\r\n\r\nBy the end of the session, you will be up-to-date with what is happening in the field."",
              ""startsAt"": ""2019-06-21T13:40:00"",
              ""endsAt"": ""2019-06-21T14:40:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""61a48ebe-ebe0-427f-a1a1-eef2f7e92ed9"",
                  ""name"": ""Oleksandra Sopova""
                },
                {
                  ""id"": ""5937af3f-6906-46e8-86a9-0e9cefd5a080"",
                  ""name"": ""Natalia An""
                }
              ],
              ""categories"": [],
              ""roomId"": 4479,
              ""room"": ""Room 1""
            },
            ""index"": 0
          },
          {
            ""id"": 4480,
            ""name"": ""Room 2"",
            ""session"": {
              ""id"": ""99876"",
              ""title"": ""Lightning Talks (Diverse)"",
              ""description"": ""Lightning talks (approx 10-15 minutes each)\r\n\r\nTalk 1: 5 Lessons Learned from Implementing 40+ Machine Learning Projects - Xiaopeng Li\r\n\r\nMachine learning has reached the peak of Gartner's hype curve in 2018. Many companies are talking about it, while very few are actually doing it. At Inmeta, we have together with partners and clients implemented 40+ machine learning projects in the past few years. In this talk, I will share the top 5 lessons we learned from doing machine learning for real.\r\n\r\n-----------------------------------------------------------------------\r\n\r\nTalk 2: test && commit || revert. What?! - Kari Eline Strandjord\r\n\r\n`test && commit || revert` is a workflow developed during a week of code camp at Iterate with Kent Beck. The main idea is that your code should always be in a valid state. If the test passes, the code is committed. If it fails, you lose all your changes, and the code is forced back to the last valid state. At first, the idea seemed both unrealistic and a bit harsh. But after trying it out during a longer period writing an application in Elm, it did change my way of coding. \r\n\r\nThe talk will give a brief explanation of the workflow. I will also share my experiences from using it in a real project.\r\n\r\n-----------------------------------------------------------------------\r\n\r\nTalk 3: Five Ways to Break a Git Repository - Edward Thomson\r\n\r\nCan you break your Git repository? I hope not! That's where you keep all your stuff! Edward Thomson shows you five common mistakes that break Git repositories and how to fix them.\r\n\r\n-----------------------------------------------------------------------\r\n\r\nTalk 4: Unlocking the doors of parliament - Sindre Lindstad\r\n\r\nWhen Norway's Minister of Children and Families was instated, he enthusiastically showed the world the key to his new office through press photos.\r\n\r\nThe only problem was that it was a plastic punch-hole keycard, which meant anyone could make a copy.\r\n\r\nSo I made one with 3D printing (and lasers!). But does it work?\r\n\r\n----------------------------------------------------------------------- "",
              ""startsAt"": ""2019-06-21T13:40:00"",
              ""endsAt"": ""2019-06-21T14:40:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""99aa52b8-e0ab-4bd8-be19-437cc4b93740"",
                  ""name"": ""Xiaopeng Li""
                },
                {
                  ""id"": ""a5357148-b11b-4ab0-822c-b650881773ef"",
                  ""name"": ""Kari Eline Strandjord""
                },
                {
                  ""id"": ""fee375b8-047c-4ea2-ab43-9837f2420b19"",
                  ""name"": ""Edward Thomson""
                },
                {
                  ""id"": ""5ec0ca76-bf24-4fce-bf3d-80656dadf1f6"",
                  ""name"": ""Sindre Lindstad""
                }
              ],
              ""categories"": [],
              ""roomId"": 4480,
              ""room"": ""Room 2""
            },
            ""index"": 1
          },
          {
            ""id"": 4481,
            ""name"": ""Room 3"",
            ""session"": {
              ""id"": ""98611"",
              ""title"": ""An in-flight port from Angular to React, a tale of performance and happiness"",
              ""description"": ""A real-world story of how we used some clever trickery to completely rewrite an app, bit by bit from Angular to React, resulting in better performance, a smaller footprint, a shorter feedback loop, less coupling, fewer bugs, increased development velocity and happier developers.\r\nWe did the re-write, while deploying to production frequently. New features were added to the product throughout the whole rewrite process, and stability was maintained throughout the entire process.\r\n\r\nThe application is an e-commerce payment solution (Nets Easy), used by numerous merchants in their web shops to get paid."",
              ""startsAt"": ""2019-06-21T13:40:00"",
              ""endsAt"": ""2019-06-21T14:40:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""912bbb93-e7bd-4448-8854-19ef39fa5843"",
                  ""name"": ""Henning Christiansen""
                },
                {
                  ""id"": ""1e49e725-0923-4dae-b00d-f443c518b010"",
                  ""name"": ""Francis Paulin""
                }
              ],
              ""categories"": [],
              ""roomId"": 4481,
              ""room"": ""Room 3""
            },
            ""index"": 2
          }
        ]
      },
      {
        ""slotStart"": ""15:00:00"",
        ""rooms"": [
          {
            ""id"": 4479,
            ""name"": ""Room 1"",
            ""session"": {
              ""id"": ""98658"",
              ""title"": ""The Hitchhiker's Guide to the Cloud (AWS vs GCP vs Azure) and their AI/ML API’s capabilities"",
              ""description"": ""To companies leveraging the capabilities of public cloud (often Amazon Web Services, Google Cloud or Microsoft Azure) the felling of immersion into a single provider platform is constant in their day to day. With a rapid evolution of services becoming available in each cloud provider, companies tend to focus and keep updated with only one of them while other providers capabilities are simply unknown, ignored or forgotten. \r\nOn the other hand, there are many companies that are not yet using public cloud and are now facing the dilemma of which Public Cloud provider to choose.\r\n\r\nAI and Machine Learning are key areas of investment, growth and differentiation for many companies and that is no exception for the three biggest public cloud players (AWS, GCP and Azure). In this context, pre-trained AI/ML API’s in combination with other Serverless services is one area that has been on the rise and with fast adoption. \r\n\r\nIn this talk we will learn about the three major public cloud providers (AWS, GCP and Azure) by having an overview and gain insights about each other pros and cons. In addition, we are going to explore their AI/ML Cloud API’s that allow us to leverage ready-made capabilities such as: Text to Speech, Image & Video Classification, Translation,  Speech Recognition, Sentiment Analysis, etc.\r\n\r\n"",
              ""startsAt"": ""2019-06-21T15:00:00"",
              ""endsAt"": ""2019-06-21T16:00:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""a2e6985d-7ead-4b43-bdde-bfdea9923300"",
                  ""name"": ""Bruno Amaro Almeida""
                }
              ],
              ""categories"": [],
              ""roomId"": 4479,
              ""room"": ""Room 1""
            },
            ""index"": 0
          },
          {
            ""id"": 4480,
            ""name"": ""Room 2"",
            ""session"": {
              ""id"": ""99320"",
              ""title"": ""Trying to learn C#"",
              ""description"": ""Learning a new language is often colored by the language you come from. As a programmer coming from C++ and Java, with some functional programming background, how did I navigate trying to get a grasp of C#? Should be fun for C# developers, but also educational: How do we teach a new language to folks that already know how to program?"",
              ""startsAt"": ""2019-06-21T15:00:00"",
              ""endsAt"": ""2019-06-21T16:00:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""0eaa4bb2-cb2a-4b76-800d-de8b1dfdb50c"",
                  ""name"": ""Patricia Aas""
                }
              ],
              ""categories"": [],
              ""roomId"": 4480,
              ""room"": ""Room 2""
            },
            ""index"": 1
          },
          {
            ""id"": 4481,
            ""name"": ""Room 3"",
            ""session"": {
              ""id"": ""99143"",
              ""title"": ""It's about time"",
              ""description"": ""Time Zones, Daylight savings, Leap years, Leap seconds... Storing it all, testing it, getting it right for every point in time in every country... \r\nWriting correct timing code can be a nightmare! \r\nWe'll be ranting our way through some common pitfalls, tips and tricks to enable you to reason more effectively about time in your applications."",
              ""startsAt"": ""2019-06-21T15:00:00"",
              ""endsAt"": ""2019-06-21T16:00:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""d28ae055-13a5-4d0a-8668-24fd93198cef"",
                  ""name"": ""Christin Gorman""
                }
              ],
              ""categories"": [],
              ""roomId"": 4481,
              ""room"": ""Room 3""
            },
            ""index"": 2
          }
        ]
      },
      {
        ""slotStart"": ""16:20:00"",
        ""rooms"": [
          {
            ""id"": 4479,
            ""name"": ""Room 1"",
            ""session"": {
              ""id"": ""99155"",
              ""title"": ""Everything is Cyber-broken 2"",
              ""description"": ""TBA - submitting this now so you have it in the agenda, it'll be an all new talk in the theme of the first cyber-broken talk"",
              ""startsAt"": ""2019-06-21T16:20:00"",
              ""endsAt"": ""2019-06-21T17:20:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""735a4b60-42e8-4452-9480-68197372c206"",
                  ""name"": ""Troy Hunt""
                }
              ],
              ""categories"": [],
              ""roomId"": 4479,
              ""room"": ""Room 1""
            },
            ""index"": 0
          },
          {
            ""id"": 4480,
            ""name"": ""Room 2"",
            ""session"": {
              ""id"": ""86521"",
              ""title"": ""Rediscovering fire - on designing portable, multi-language libraries"",
              ""description"": ""The session will cover the design choices and lessons learned developing the multi-language free library segyio, or more conceptually, designing libraries for libraries.\r\n\r\nBriefly, it will discuss:\r\n- Stable API, ABI, and how to design them for the future\r\n- How to design C-interface libraries that allows for good foreign-language libraries (in our case python)\r\n- Library design philosophy and the beauty of primitive functions\r\n- How to design for composition and caller flexibility\r\n- Plumbing and porcelain\r\n\r\nThe session should appeal both to library developers for embedded systems, and consumers of higher-level libraries in desktop and scientific applications, as the topic covered is the bridge between primitive and sophisticated systems, and making it beautiful."",
              ""startsAt"": ""2019-06-21T16:20:00"",
              ""endsAt"": ""2019-06-21T17:20:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""142b682d-b548-423c-8e51-e36ee08db70f"",
                  ""name"": ""Jørgen Kvalsvik""
                }
              ],
              ""categories"": [],
              ""roomId"": 4480,
              ""room"": ""Room 2""
            },
            ""index"": 1
          },
          {
            ""id"": 4481,
            ""name"": ""Room 3"",
            ""session"": {
              ""id"": ""99253"",
              ""title"": ""Who's Who? Federating Identity with Azure B2C"",
              ""description"": ""Often, users of your system will already know who they are, or at least think they do. Making sure you know who they are and what they can do is pretty important too.\r\n\r\nIn this session Microsoft Engineer Andrew Coates will present techniques for allowing users to log into your system with credentials from another system. Using Azure B2C allows you to offload authentication to other identity providers while keeping authorization tasks local to your system.\r\n\r\nOffload the hassles of lost passwords, expiring accounts and more, leaving you time to build and maintain the things that are important to your system.\r\n\r\nAndrew will demonstrate the setup and configuration of this powerful identity federation system allowing integration of any combination of social identities such as Facebook or twitter, as well as organisational accounts like Active Directory and others. He'll also discuss the extension points allowing complete control of the identity system including rules-based identity flows and calling out to custom REST services as part of the claims processing flow,\r\n\r\nIf your system needs to include users from outside your organisation, this is a must-see session.\r\n"",
              ""startsAt"": ""2019-06-21T16:20:00"",
              ""endsAt"": ""2019-06-21T17:20:00"",
              ""isServiceSession"": false,
              ""isPlenumSession"": false,
              ""speakers"": [
                {
                  ""id"": ""2809c750-dc83-4e1c-8f06-38ee96b818b6"",
                  ""name"": ""Andrew Coates""
                }
              ],
              ""categories"": [],
              ""roomId"": 4481,
              ""room"": ""Room 3""
            },
            ""index"": 2
          }
        ]
      }
    ]
  }
]";
}
