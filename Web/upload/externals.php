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
	
		echo @file_get_contents("http://127.0.0.1/RELEASE63-35255-34886-201108111108_ce2d130905ba279edbfb4208cd5035c0/external_variables.txt");	
		$get = mysql_query("SELECT * FROM external_variables");
		
		while ($ext = mysql_fetch_assoc($get))
		{
			$GetTemplate->WriteLine($ext['skey'] . '=' . $ext['sval'] . LB);
		}
		
		break;

	case "external_flash_texts":
	
		echo @file_get_contents("http://127.0.0.1/RELEASE63-35255-34886-201108111108_ce2d130905ba279edbfb4208cd5035c0/external_flash_texts.txt");	
		$get = mysql_query("SELECT * FROM external_texts");
		
		while ($ext = mysql_fetch_assoc($get))
		{
			$GetTemplate->WriteLine($ext['skey'] . '=' . $ext['sval'] . LB);
		}
		
		break;
}

?>