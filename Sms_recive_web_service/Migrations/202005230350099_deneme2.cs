namespace Sms_recive_web_service.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class deneme2 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.door_history",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        person_photo = c.String(unicode: false),
                        Time = c.DateTime(nullable: false, precision: 0),
                        Door_case = c.Int(nullable: false),
                        ApplicationUser_Id = c.String(maxLength: 128, storeType: "nvarchar"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUser_Id)
                .Index(t => t.ApplicationUser_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.door_history", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropIndex("dbo.door_history", new[] { "ApplicationUser_Id" });
            DropTable("dbo.door_history");
        }
    }
}
