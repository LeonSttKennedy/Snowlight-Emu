<?php   
/*==========================================================================\
| RainbowCMS - A good CMS option for Snowlight Emulator                     |
| Copyright (C) 2021 - Created by LeoXDR edited by Souza                    |
| https://github.com/LeonSttKennedy/Snowlight-Emu                           |
| ========================================================================= |
| This CMS was developed with the permission of Meth0d                      |
\==========================================================================*/

include_once 'database.config.php';

class MysqlConnection
{
	function __construct() 
	{
		$mysql_connection = mysql_connect(DB_SERVER, DB_USERNAME, DB_PASSWORD) or die("<center>RainbowCMS Message<br><br><font color='red'><h2>Review your configuration file.</h2></font></center>");
		mysql_select_db(DB_NAME, $mysql_connection) or die('Oops ! We have a problem in the db, check the config file ! Error: -> ' . mysql_error());
	}
	
	function CheckIfConfigFileExists($filepath)
	{
		if (!file_exists($filepath))
{
$config_file = "<?php   
/*==========================================================================\
| RainbowCMS - A good CMS option for Snowlight Emulator                     |
| Copyright (C) 2021 - Created by LeoXDR edited by Souza                    |
| https://github.com/LeonSttKennedy/Snowlight-Emu                           |
| ========================================================================= |
| This CMS was developed with the permission of Meth0d                      |
\==========================================================================*/

	// Database Configuration
	
	// Host
	define('DB_SERVER', 'localhost');

	// User
	define('DB_USERNAME', 'root');

	// Password
	define('DB_PASSWORD', 'pass');
	
	// Database
	define('DB_NAME', 'snowdb');
?>";

$gen_now = fopen($filepath,"w");
fwrite($gen_now,"$config_file");
fclose($gen_now);

die("<center><h2>RainbowCMS Message<br>The Configuration File was generated now, you must edit it to the website works, go to <u>system</u> folder and search for file <u>database.config.php</u>, edit the file with your data and press F5 to update this webpage.</h2></center>");
}
	}
	
	function CheckConfigurationData($db_hostname, $db_username, $db_password, $db_name)
	{
		if($db_hostname == "" || $db_username == "" || $db_password == "" || $db_name == "")
		{
			$Error_Text = "Oops ! We have a problem, all configuration fields are blank.";
			die ('<center><h2>'.$Error_Text.'</h2></center>');
		}
		else		{		return;		}
	}
	
	function DatabaseCleanUp()
	{
		
	}
	
	function execQuery_($query)
	{
		mysql_query(''.$query.'');
	}
	
}


?>