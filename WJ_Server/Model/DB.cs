using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dos.ORM.Common;
using Dos.ORM;

public class Db
{
    public static readonly DbSession Context = new DbSession(DatabaseType.SqlServer, "data source=.;initial catalog=RS_PIS;user id=wj;pwd=Mc111111");
}

