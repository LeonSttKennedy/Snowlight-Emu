<?php
/*==========================================================================\
| RainbowCMS - A good CMS option for Snowlight Emulator                     |
| Copyright (C) 2021 - Created by LeoXDR edited by Souza                    |
| https://github.com/LeonSttKennedy/Snowlight-Emu                           |
| ========================================================================= |
| This CMS was developed with the permission of Meth0d                      |
\==========================================================================*/

require_once "brain.php";

$id = '';

if (isset($_GET['id']))
{
	$id = $_GET['id'];
}

switch ($id)
{
	case "external_variables":
	
		echo @file_get_contents("http://192.168.15.72/cdn.classichabbo.com/r38/gordon/RELEASE63-34888-34886-201107192308_9e5b377e2ee4333b61eb9d20d356936d/external_variables.txt");	
		$get = mysql_query("SELECT * FROM external_variables");
		
		while ($ext = mysql_fetch_assoc($get))
		{
			echo $ext['skey'] . '=' . $ext['sval'] . LB;
		}
		
		break;

	case "external_flash_texts":
	
		echo @file_get_contents("http://192.168.15.72/cdn.classichabbo.com/r38/gordon/RELEASE63-34888-34886-201107192308_9e5b377e2ee4333b61eb9d20d356936d/external_flash_texts.txt");	
		$get = mysql_query("SELECT * FROM external_texts");
		
		while ($ext = mysql_fetch_assoc($get))
		{
			echo $ext['skey'] . '=' . $ext['sval'] . LB;
		}
		
		break;
}

?>