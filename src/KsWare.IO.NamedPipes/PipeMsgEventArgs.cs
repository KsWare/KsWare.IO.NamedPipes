﻿// ***********************************************************************
// Assembly         : KsWare.IO.NamedPipes
// Author           : SchreinerK
// Created          : 02-02-2018
//
// Last Modified By : SchreinerK
// Last Modified On : 02-03-2018
// ***********************************************************************
// <copyright file="PipeMsgEventArgs.cs" company="KsWare">
//     Copyright © 2018 by KsWare. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;

namespace KsWare.IO.NamedPipes {

	/// <summary>
	/// Class PipeMsgEventArgs.
	/// </summary>
	/// <seealso cref="System.EventArgs" />
	/// <autogeneratedoc />
	public class PipeMsgEventArgs : EventArgs {

		/// <summary>
		/// Initializes a new instance of the <see cref="PipeMsgEventArgs"/> class.
		/// </summary>
		/// <autogeneratedoc />
		public PipeMsgEventArgs() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="PipeMsgEventArgs"/> class.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <autogeneratedoc />
		public PipeMsgEventArgs(string request) {
			Request = request;
		}

		/// <summary>
		/// Gets the request.
		/// </summary>
		/// <value>The request.</value>
		/// <autogeneratedoc />
		public string Request { get; }

		/// <summary>
		/// Gets or sets the response.
		/// </summary>
		/// <value>The response.</value>
		/// <autogeneratedoc />
		public string Response { get; set; }


	}

}
