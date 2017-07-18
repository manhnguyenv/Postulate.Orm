# Postulate ORM

Postulate is a lightweight code-first ORM for SQL Server made with Dapper. It offers simple, robust CRUD operations, some extras like [change](https://github.com/adamosoftware/Postulate.Orm/wiki/Change-Tracking) and [delete](https://github.com/adamosoftware/Postulate.Orm/wiki/Change-Tracking) tracking, and uniquely -- a very convenient way to synchronize your model classes to your database using an .exe run in your build event.

For a video demo, please see this at [Vimeo.com](https://vimeo.com/219400011). Also see the [CodeProject.com article here](https://www.codeproject.com/Articles/1191399/Intro-to-Postulate-ORM).

To get started:

1. Create a solution with two projects. One will be for model classes only, the other is your main application, which can be any type. (Creating a separate project for model classes  is a good design choice to begin with, but it also simply works better for Postulate because the Schema Merge app avoids some reflection errors if the models are in their own project.)

2. Install nuget package **Postulate.Orm** in both projects in your solution.

3. Install the Schema Merge app from the [MergeUI releases](https://github.com/adamosoftware/Postulate.MergeUI/releases) page. This will create a desktop icon that looks like this:

    ![img](https://adamosoftware.blob.core.windows.net/images/schema_merge_icon.png)

4. In the post build event of your **models** project, enter this command line:

`"C:\Users\Adam\AppData\Local\Adam O'Neil Software\Postulate Schema Merge\PostulateMergeUI.exe" "$(TargetPath)"`

5. In your models project, add a config file with a `ConnectionString` element with a valid SQL Server connection. The database does not have to exist, but the server and credentials must be valid. This example assumes a connection named `DefaultConnection`.

6. In your models project, create a class like this that represents your "root" database object, sort of like a DbContext in Entity Framework. This example uses `MyDb`, but you would choose a name that better reflects what your application is about. Notice there are two constructors.

    ```
    namespace Models
    {
        public class MyDb : SqlServerDb<int>
        {
            public MyDb() : base("DefaultConnection")
            {
            }

            public MyDb(Configuration config) : base(config, "DefaultConnection")
            {
            }
        }
    }
    ```
7. Create one or more model classes that share the same namespace as your `MyDb` class. In the example above, the namespace is `Models`, so all of your model classes that you create subsequently need to be in that namespace. See [this topic](https://github.com/adamosoftware/Postulate.Orm/wiki/Designing-Model-Classes) on creating model classes for Postulate.

Whenever you build or rebuild your models project, the Schema Merge app will run, and you should see a window like this. Click the **Execute** button in the upper right to apply changes in your model classes to your database. Postulate can detect a range of changes, and generate correct SQL to implement them. Postulate will never drop tables that contain data.

![img](https://adamosoftware.blob.core.windows.net:443/images/schema_merge_app.png)

## Examples

The following examples assume a `SqlServerDb<int>` variable called `MyDb` and this model class:

    public class Customer : Record<int>
    {
      [ForeignKey(typeof(Organization))]
      public int OrganizationId { get; set; }
      [MaxLength(50)]
      [Required]
      public string FirstName { get; set; }
      [MaxLength(50)]
      [Required]
      public string LastName { get; set; }
      public string Address { get; set; }
      public string City { get; set; }
      [MaxLength(2)]
      public string State { get; set; }
      [MaxLength(10)]
      public string ZipCode { get; set; }
    }

Find a Customer record from a key value.

    var customer = new MyDb().Find<Customer>(id);
    
Create and save a new Customer. "Save" here means an insert or update is performed automatically according to the state of the record:

    var customer = new Customer() { FirstName = "Adam", LastName = "O'Neil" };
    new MyDb().Save<Customer>(customer);

Find a Customer based on a WHERE clause:

    var customer = new MyDb().FindWhere<Customer>(
      "[LastName]=@lastName AND [FirstName]=@firstName", 
      new { lastName = "O'Neil", firstName = "Adam" });
      
Update select properties of a Customer without updating the whole record:

    customer.Address = "3232 Whatever St";
    customer.City = "Binghamton Heights";
    customer.State = "XR";
    customer.ZipCode = "12345";
    new MyDb().Update<Customer>(customer, r => r.Address, r => r.City, r => r.State, r => r.ZipCode);

### ASP.NET MVC Suggestions

I recommend putting your model classes in a separate project/DLL from your web app. Mysterious ReflectionTypeLoadException can happen in MVC, and it's also a good practice to keep model classes separate from the application, anyway. The [Postulate.Mvc](https://github.com/adamosoftware/Postulate.Mvc) repo is really the best place to see Postulate in action in a more realistic site.

I recommend having a `SqlServerDb<TKey>` instance variable in your controllers.

    private _db = new MyDb();
    
Override the controller Initialize event to set the `_db.UserName` property. This enables `Record<TKey>` overrides such as [BeforeSave](https://github.com/adamosoftware/Postulate.Orm/blob/master/PostulateV1/Abstract/Record.cs#L116) and [AllowSave](https://github.com/adamosoftware/Postulate.Orm/blob/master/PostulateV1/Abstract/Record.cs#L107) to have access to the current user name without depending on any particular Identity provider.

    protected override void Initialize(RequestContext requestContext)
    {            
        base.Initialize(requestContext);
        _db.UserName = User.Identity.Name;
    }

A very simple Edit action:

    public ActionResult Edit(int id)
    {
        var customer = _db.Find<Customer>(id);
        return View(customer);
    }

A very simple Save action (for both inserts and updates):

    public ActionResult Save(Customer customer)
    {
        _db.Save<Customer>(customer);
        return RedirectToAction("Edit", new { id = customer.Id });
    }
    
If you need to open a connection manually somewhere, use the [GetConnection](https://github.com/adamosoftware/Postulate.Orm/blob/master/PostulateV1/SqlServerDb.cs#L43) method:

    using (var cn = _db.GetConnection())
    {
        cn.Open();
        
        // do stuff with the connection
        _db.Save<Customer>(cn, record);
        
        // execute SQL (with Dapper)
        cn.Execute(*blah blah blah*);
    }
 
For more realistic MVC examples, please see the [Postulate.Mvc project](https://github.com/adamosoftware/Postulate.Mvc).

If you have any ideas or feedback, please contact me at adamosoftware@gmail.com. Contributions here are definitely welcome.
