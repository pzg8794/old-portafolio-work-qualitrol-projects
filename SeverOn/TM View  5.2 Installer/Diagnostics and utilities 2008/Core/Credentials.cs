using System;

namespace Serveron.Utility.Core
{
	/// <summary>
	/// These are the possible user roles.
	/// </summary>
	[Serializable()]
	public enum UserRole
	{
		InvalidNullRole = 0,
		ServeronAdmin,
		CustomerAdmin,
		ServeronUser,	// this is the "see everything but see it view-only" role
		CustomerUser
	}

	/// <summary>
	/// Credentials representation
	/// </summary>
	[Serializable()]
	public class Credentials
	{
		#region Private state

		private string _companyName;
		private Guid _companyID;
		private string _userName;
		private string _databaseCompatibleUserName;
		private Guid _userObjectID;
		private string _userRoleName;
		private UserRole _userRole;
		private string _hashedPassword;
		private string _dpapiPassword;
		private string _firstName;
		private string _lastName;
		private string _accountDescription;

		#endregion

		#region Construction and disposal

		/// <summary>
		/// Create an immutable Credentials object from various data.
		/// </summary>
		/// <param name="companyName"></param>
		/// <param name="companyID"></param>
		/// <param name="databaseCompatibleUserName"></param>
		/// <param name="userName"></param>
		/// <param name="userRole"></param>
		/// <param name="userRoleName"></param>
		public Credentials(string companyName, Guid companyID, string userName,
			string databaseCompatibleUserName, string userRoleName, UserRole userRole):
		this(companyName, companyID, userName, databaseCompatibleUserName, userRoleName,
				userRole, Guid.Empty, null, null, userName, userName, "")
		{
		}

		/// <summary>
		/// Create an immutable Credentials object from various data.
		/// </summary>
		/// <param name="companyName"></param>
		/// <param name="companyID"></param>
		/// <param name="databaseCompatibleUserName"></param>
		/// <param name="userName"></param>
		/// <param name="userRole"></param>
		/// <param name="userRoleName"></param>
		/// <param name="userObjectID"></param>
		/// <param name="hashedPassword"></param>
		/// <param name="dpapiPassword"></param>
		public Credentials(string companyName, Guid companyID, string userName,
			string databaseCompatibleUserName, string userRoleName, UserRole userRole,
			Guid userObjectID, string hashedPassword, string dpapiPassword):
		this(companyName, companyID, userName, databaseCompatibleUserName, userRoleName,
			userRole, userObjectID, hashedPassword, dpapiPassword, userName, userName, "")

		{
		}

		/// <summary>
		/// Create an immutable Credentials object from various data.
		/// </summary>
		/// <param name="companyName"></param>
		/// <param name="companyID"></param>
		/// <param name="databaseCompatibleUserName"></param>
		/// <param name="userName"></param>
		/// <param name="userRole"></param>
		/// <param name="userRoleName"></param>
		public Credentials(string companyName, Guid companyID, string userName,
			string databaseCompatibleUserName, string userRoleName, UserRole userRole,
			Guid userObjectID, string hashedPassword, string dpapiPassword,
			string firstName, string lastName, string accountDescription)
		{
			_companyName = companyName;
			_companyID = companyID;
			_userName = userName;
			_databaseCompatibleUserName = databaseCompatibleUserName;
			_userRoleName = userRoleName;
			_userRole = userRole;
			_userObjectID = userObjectID;
			_hashedPassword = hashedPassword;
			_dpapiPassword = dpapiPassword;
			_firstName = firstName;
			_lastName = lastName;
			_accountDescription = accountDescription;
		}

		#endregion

		#region Public properties

		/// <summary>
		/// Get the name of the user's company
		/// </summary>
		public string CompanyName
		{
			get
			{
				return _companyName;
			}
		}

		/// <summary>
		/// Get the ID of the named company.
		/// </summary>
		public Guid CompanyID
		{
			get
			{
				return _companyID;
			}
		}

		/// <summary>
		/// Get the user's login
		/// </summary>
		public string UserName
		{
			get
			{
				return _userName;
			}
		}

		/// <summary>
		/// Get the user object ID, the UniqueID GUID
		/// of the user object for this user.
		/// </summary>
		public Guid UserObjectID
		{
			get
			{
				return _userObjectID;
			}
		}

		/// <summary>
		/// Get the username in database format, with the first
		/// 10 characters of the company GUID (NNNNNNNN-N) attached.
		/// </summary>
		public string DatabaseCompatibleUserName
		{
			get
			{
				return _databaseCompatibleUserName;
			}
		}

		/// <summary>
		/// Get the "qualified" name, Company\LoginID
		/// </summary>
		public string QualifiedName
		{
			get
			{
				return CompanyName + @"\" + UserName;
			}
		}

		/// <summary>
		/// Get the account's first name, or UserID if none.
		/// </summary>
		public string FirstName
		{
			get
			{
				return _firstName;
			}
		}

		/// <summary>
		/// Get the account's last name, or UserID if none.
		/// </summary>
		public string LastName
		{
			get
			{
				return _lastName;
			}
		}

		/// <summary>
		/// Get the account's description.
		/// </summary>
		public string AccountDescription
		{
			get
			{
				return _accountDescription;
			}
		}

		/// <summary>
		/// Get the name of the user's role.
		/// </summary>
		public string UserRoleName
		{
			get
			{
				return _userRoleName;
			}
		}

		/// <summary>
		/// Get the enumeration value of the user's role.
		/// This value may be referenced as either "Role" or "UserRole".
		/// </summary>
		public UserRole UserRole
		{
			get
			{
				return _userRole;
			}
		}

		/// <summary>
		/// Get the enumeration value of the user's role.
		/// This value may be referenced as either "Role" or "UserRole".
		/// </summary>
		public UserRole Role
		{
			get
			{
				return _userRole;
			}
		}

		/// <summary>
		/// Get the "hashed" (MD5 hashed at least through v3.1.0.0)
		/// password of the user.  Not necessarily set or used by
		/// every consumer of this shared class.
		/// </summary>
		public string Password
		{
			get
			{
				return _hashedPassword;
			}
		}

		/// <summary>
		/// Get the DPAPI encrypted password of the user.  Not
		/// necessarily set or used by every consumer of this shared class.
		/// </summary>
		public string DPAPIPassword
		{
			get
			{
				return _dpapiPassword;
			}
		}

		/// <summary>
		/// The password has changed.
		/// </summary>
		/// <param name="md5HashedPassword">MD5 hash of new password</param>
		/// <param name="dpapiEncryptedPassword">DPAPI encrypted new password</param>
		public void ChangePassword(string md5HashedPassword, string dpapiEncryptedPassword)
		{
			this._hashedPassword = md5HashedPassword;
			this._dpapiPassword = dpapiEncryptedPassword;
		}

		#endregion

	}
}
