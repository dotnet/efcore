module DbContextTests

open Xunit
open Microsoft.EntityFrameworkCore
open System

[<CLIMutable>]
type CliMutableRecord = {
    Id: Guid
    Title: string
}

type TodoContext() =
    inherit DbContext()
    member val Todos : DbSet<CliMutableRecord> = null with get, set

let [<Fact>] ``Can create a basic Context``() =
    use ctx = new TodoContext()
    Assert.NotNull (ctx.Todos)

