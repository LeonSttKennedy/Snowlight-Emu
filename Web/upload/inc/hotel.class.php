<?php   
/*==========================================================================\
| RainbowCMS - A good CMS option for Snowlight Emulator                     |
| Copyright (C) 2021 - Created by LeoXDR edited by Souza                    |
| https://github.com/LeonSttKennedy/Snowlight-Emu                           |
| ========================================================================= |
| This CMS was developed with the permission of Meth0d                      |
\==========================================================================*/

require_once 'users.class.php';

class Hotel extends Users
{
	public function UsersOnline()
	{
		$query_serverstatus = mysql_query("SELECT * FROM server_statistics LIMIT 1") or die(mysql_error());
		$row = mysql_fetch_assoc($query_serverstatus);

		$server_status = $row["server_status"];
		$amount_users_online = $row["active_connections"];
		do
		{
			switch($server_status)
			{
				case 2:
				case 0:
					echo "The hotel is offline.";
					break;
				
				case 1:
					echo ($amount_users_online > 1 ? $amount_users_online." User(s) Online!" : $amount_users_online." User Online!");
					break;

				default:
					echo "Unknown";
					break;
			}
		}
		while(0);
	}
	
	public function GetDatabaseConfig($configrequested)
	{
		$ConfigsTable = mysql_query("SELECT * FROM site_config LIMIT 1");
		$ConfigAssoc = mysql_fetch_assoc($ConfigsTable);
		$ConfigToReturn = $ConfigAssoc["$configrequested"];
		return $ConfigToReturn;
	}
	
	public function _RandomString_($length, $numbers, $upper)
	{
		if (1 > $length) $length = 8;
		$chars = '0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ';
		$numChars = 62;
		
		if (!$numbers)
		{
			$numChars = 52;
			$chars = substr($chars, 10, $numChars);
		}
			
			if (!$upper)
			{
				$numChars -= 26;
				$chars = substr($chars, 0, $numChars);
			}
				
				$string = '';
				for ($i = 0; $i < $length; $i++)
				{
					$string .= $chars[mt_rand(0, $numChars - 1)];
				}
					
			return $string;
					
		}
	
	public function GenerateSSO($username, $text_mode = strtoupper, $tick_pass = 'RainbowCMS', $separator = '-', $size_1 = 10, $size_2 = 10, $size_3 = 9, $size_4 = 10)
	{
		$randnum1 = $this->_RandomString_($size_1, TRUE, FALSE);
		$randnum2 = $this->_RandomString_($size_4, FALSE, TRUE);
		$randnum3 = $this->_RandomString_($size_3, TRUE, TRUE);
		$randnum4 = $this->_RandomString_($size_2, TRUE, TRUE);

		$sso = ''.$tick_pass.'';
		$sso .= ''.$separator.''.$randnum1.'';
		$sso .= ''.$separator.''.$randnum2.'';
		$sso .= ''.$separator.''.$randnum3.'';
		$sso .= ''.$separator.''.$randnum4.'';

		$Ticket_SSO = $text_mode($sso);
		
		return $Ticket_SSO;
	}

}
?>