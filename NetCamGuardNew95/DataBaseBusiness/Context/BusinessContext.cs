using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using MySql.Data.EntityFrameworkCore.Extensions;

namespace DataBaseBusiness.Models
{
    public partial class BusinessContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string CONNECTION_STRING = AppSetting.GetConfig("ConnectionStrings:mySqlConn_Business");
            if (!optionsBuilder.IsConfigured)
            {
                //optionsBuilder.UseMySql("Data Source=localhost;Database=camera_guard_business;User ID=root;Password=Admin@62595738;pooling=true;port=3306;sslmode=none;CharSet=utf8;");
                optionsBuilder.UseMySql(CONNECTION_STRING);
            }
        }
    }
}
