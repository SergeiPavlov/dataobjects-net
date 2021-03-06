// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.17

using System;
using System.Diagnostics;
using Xtensive.Sql.Compiler;

namespace Xtensive.Sql.Drivers.Oracle.v11
{
  internal class Compiler : v10.Compiler
  {
    // Constructors

    protected internal Compiler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}