namespace Sms_recive_web_service.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class denemeee : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.door_history", "Time", c => c.String(unicode: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.door_history", "Time", c => c.DateTime(nullable: false, precision: 0));
        }
    }
}
