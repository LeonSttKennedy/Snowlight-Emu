<?php   
/*==========================================================================\
| RainbowCMS - A good CMS option for Snowlight Emulator                     |
| Copyright (C) 2021 - Created by LeoXDR edited by Souza                    |
| https://github.com/LeonSttKennedy/Snowlight-Emu                           |
| ========================================================================= |
| This CMS was developed with the permission of Meth0d                      |
\==========================================================================*/

require_once "security.class.php";
require_once "connection.class.php";
require_once "website.config.php";
require_once "hotel.class.php";

class Users extends Security
{
	public function TryLogin($email, $password)
	{
		Users::Enter($email, $password);
	}
	
	public function Register($entered_email, $entered_username, $entered_password, $entered_password2)
	{
		// Are empty fields?
		if(empty($entered_email) || empty($entered_username) || empty($entered_password) || empty($entered_password2))
		{
			Security::CreateSession("error", "2");
			Security::Redirect("register.php");
			exit;
		}
		
		// Check if entered username is prohibithed
		if(Users::CheckPN($entered_username) != 0)
		{
			if(preg_match('/^[a-za-zA-Z\d_]{4,32}$/i', $entered_username))
			{
			}
		}
	
		// Check if entered username is taken
		if(Users::CheckName($entered_username))
		{
			if(preg_match('/^[a-za-zA-Z\d_]{4,32}$/i', $entered_username))
			{
			}
		}
		
		// Check if IP is banned
		
		// Check if entered email is taken
		if(Users::CheckEmail($entered_email))
		{
			if(preg_match('/^[a-z0-9_\.-]+@([a-z0-9]+([\-]+[a-z0-9]+)*\.)+[a-z]{2,7}$/i', $entered_email))
			{
			}
		}
		
		// Check if entered passwords doesn't match
		if($entered_password2 != $entered_password)
		{
			Security::CreateSession("error", "4");
			Security::Redirect("register.php");
			exit;
		}
		
		$sha1_password = strtolower(Security::Encrypt('SHA1', $entered_password));
		
		// Initial users info
		$initial_figure = "hr-3194-40-31.cc-3039-100.sh-290-62.hd-3092-1.lg-270-110.fa-1206-62.ha-3129-100"; // Figure
		$initial_motto = "uberHotel Newbie"; // Motto
		$initial_credits = "100"; // Credits
		$initial_pixels = "1500"; // Pixels

		mysql_query("INSERT INTO users (account_name, account_password, account_email) VALUES ('".$entered_username."', '".$sha1_password."', '".$entered_email."');");
		$last = mysql_insert_id();
		mysql_query("INSERT INTO characters (id, account_uid,username, motto, figure, credits_balance, activity_points_balance) VALUES ('".$last."', '".$last."', '".$entered_username."', '".$initial_motto."', '".$initial_figure."', '".$initial_credits."', '".$initial_pixels."');");
		
		Users::TryLogin($entered_email, $entered_password);
	}
	
	public function CheckEmail($entered_email)
	{
		$query = mysql_query("SELECT * FROM users WHERE account_email = '".$entered_email."' LIMIT 1;");
		$ocuped = mysql_num_rows($query);
		if($ocuped == 1)
		{
			Security::CreateSession("error", "5");
			Security::Redirect("register.php");
			exit;
		}
		else
		{
		    $var = 1;
  		    return $var;
		}
	}
	
	public function CheckName($entered_username)
	{
		$query = mysql_query("SELECT * FROM users WHERE account_name = '".$entered_username."' LIMIT 1;");
		$ocuped = mysql_num_rows($query);
		if($ocuped == 1)
		{
			Security::CreateSession("error", "3");
			Security::Redirect("register.php");
			exit;
		}
		else
		{
		    $var = 1;
  		    return $var;
		}
	}
	
	public function CheckPN($entered_username)
	{
		$prohibited_names = array('leoxgr', 'chuck_norris', 'norris', 'leo', 'at0m', 'moderator', 'administrador', 'Beta', 'Snowlight', 'meth0d', 'method', 'hack', 'uber', 'ion', 'nillus', 'nils', 'roy', 'hotel', 'staff');
		
		if (in_array($entered_username,$prohibited_names)) 
		{
			Security::CreateSession("error", "1");
			Security::Redirect("register.php");
			exit;
		}
		else 
		{
		    $var = 1;
  		    return $var;
        }
	}
	
	public function CreateUser($email, $username, $password, $result = FALSE)
	{
		$reg = $this->CreateCharacter($username);
		if($reg != $result)
		{
			Security::Redirect("success.php");
		}
	}
	
	public function Enter($email, $password)
	{
		$sha1_password = strtolower(Security::Encrypt('SHA1', $password));
		
		$result = mysql_query("SELECT * FROM users WHERE account_email = '$email' LIMIT 1");
		
		$user_data = mysql_fetch_array($result);
		$no_rows = mysql_num_rows($result);
		if(empty($email) && empty($password) || empty($email) || empty($password))
		{
			Security::CreateSession("login_error", "1");
			Security::Redirect("login.php");
			exit;
		}
		else
		{
			if ($no_rows >= 1) 
			{
				if($sha1_password == $user_data["account_password"])
				{
					$_SESSION['login'] = true;
					$_SESSION['id'] = $user_data['id'];
					$_SESSION['account_name'] = $user_data['account_name'];
					
					Security::Redirect("me.php");
				}
				else if($user_data["account_password"] != $sha1_password)
				{
					Security::CreateSession("login_error", "2");
					Security::Redirect("login.php");
					exit;
				}
			}
			else if($no_rows <= 0)
			{
				Security::CreateSession("login_error", "2");
				Security::Redirect("login.php");
				exit;
			}
		}
	}

	public function CreateCharacter($char_name)
	{
		$GetConnect = new MysqlConnection();
		
		$initial_figure = "hr-3194-40-31.cc-3039-100.sh-290-62.hd-3092-1.lg-270-110.fa-1206-62.ha-3129-100";
		$initial_motto = "uberHotel Newbie";
		$initial_credits = "100";
		$initial_pixels = "1500";
		$initial_respects = "3";

		$InsertInDatabase = mysql_query("INSERT INTO Persons (FirstName, LastName, Age) VALUES ('Peter', 'Griffin', '35')");

		if($InsertInDatabase)
		{
			return TRUE;
		}
	}
		
	public function BanUser($id){}	
	public function UpdateUser($id, $table, $values){}
	public function DeleteUser($id){}
	public function CheckSubscriptionsHC($id){}
	
	public function SSO($username)
	{
		$GetHotel = new Hotel();		
		
		$SSO_Data = $GetHotel->GenerateSSO($username);
		
		$this->UpdateSSO($SSO_Data, $username, 1);
		
		return $SSO_Data;
	}
	
	public function UpdateSSO($SSO, $username, $mode)
	{
        mysql_query("UPDATE characters SET auth_ticket = '$SSO' WHERE username = '$username'");
	}
	
	public function GetUserValue($uid, $value)
	{
		$result = mysql_query("SELECT $value FROM characters WHERE account_uid = '$uid' LIMIT 1");
		$user_data = mysql_fetch_array($result);
		echo $user_data[''.$value.''];
	}
	
	public function HasRight($user_name, $required_right)
	{
		$toreturn = false;
		
		$UserQuery = mysql_query("SELECT * FROM characters WHERE username = '$user_name' LIMIT 1");
		$UserGet = mysql_fetch_assoc($UserQuery);
		$UserId = $UserGet['id'];

		$BadgesQuery = mysql_query("SELECT badge_id FROM badges WHERE user_id = $UserId");
		while($BadgesGet = mysql_fetch_array($BadgesQuery))
		{
			for($B = 0; $B < count($BadgesGet); $B++)
			{
				$DefinitionsQuery = mysql_query("SELECT * FROM badge_definitions WHERE id = $BadgesGet[$B] LIMIT 1");
				$DefinitionsGet = mysql_fetch_array($DefinitionsQuery);
				$DefinitionsArray = explode(",", $DefinitionsGet["rights_sets"]);
				foreach($DefinitionsArray as $values)
				{
					$RightsQuery = mysql_query("SELECT right_id FROM rights WHERE set_id = '$values'");
					while($RightsGet = mysql_fetch_array($RightsQuery))
					{
						for($R = 0; $R < count($RightsGet); $R++)
						{
							if($RightsGet[$R] == $required_right)
							{
								$toreturn = true;
							}
						}
					}
				}
			}
		}
		
		return $toreturn;
	}
}
?>