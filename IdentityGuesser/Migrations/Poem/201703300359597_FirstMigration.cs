namespace IdentityGuesser.Migrations.Poem
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FirstMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PoemModels",
                c => new
                    {
                        PoemModelNumber = c.Int(nullable: false, identity: true),
                        ImagePath = c.String(),
                        Age = c.String(),
                        Gender = c.String(),
                        Caption = c.String(),
                        Tags = c.String(),
                    })
                .PrimaryKey(t => t.PoemModelNumber);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.PoemModels");
        }
    }
}
