<?php   
/*==========================================================================\
| RainbowCMS - A good CMS option for Snowlight Emulator                     |
| Copyright (C) 2021 - Created by LeoXDR edited by Souza                    |
| https://github.com/LeonSttKennedy/Snowlight-Emu                           |
| ========================================================================= |
| This CMS was developed with the permission of Meth0d                      |
\==========================================================================*/

header('Content-Type: text/html; charset=iso-8859-1');

define('DS', DIRECTORY_SEPARATOR);
define('LB', chr(13));
define('CWD', str_replace('manage' . DS, '', dirname(__FILE__) . DS));
define('INCLUDES', CWD . 'inc' . DS);

function usingClass($file){ require_once INCLUDES . $file.'.class.php'; }
function usingConfig($file){ require_once INCLUDES . $file.'.config.php'; }

######################################################################

usingClass("connection");
usingClass("core");
usingClass("hotel");
usingClass("security");
usingClass("users");
usingClass("template");

usingConfig("database");
usingConfig("client");
usingConfig("website");

$GetConnection = new MysqlConnection(); 
$GetCore = new Core(); 
$GetTemplate = new TemplateManager();
$GetUsers = new Users();
$GetHotel = new Hotel(); 
$GetSecurity = new Security();

######################################################################

$GetSecurity->StartSession();

$GetTemplate->Init();

$GetConnection->CheckIfConfigFileExists(INCLUDES . "database.config.php");
$GetConnection->CheckConfigurationData(DB_SERVER, DB_USERNAME, DB_PASSWORD, DB_NAME);

define('REG_STATUS', Hotel::GetDatabaseConfig("reg_status"));
define('MAINT_STATUS', ((Hotel::GetDatabaseConfig("maintenance") == "1") ? true : false));

if(MAINT_STATUS && !defined('IN_MAINTENANCE'))
{
	if(!$GetTemplate->isLogged() || !$GetUsers->HasRight($GetUsers->Name2Id($_SESSION['account_name']), "fuse_ignore_maintenance"))
	{
		header('Location: http://' . SITE_DOMAIN . '/maintenance.php'); 
		exit;
	}
}

function CheckUsersOnline() {  $CheckServer = new Hotel();  $CheckServer->UsersOnline(); }

function GetUserInfo($uid, $value) {  $GetUsers = new Users();  $GetUsers->GetUserValue($uid, $value); }

function Texts($filename) { $GetTexts = new TemplateManager();  $GetTexts->GetTEXT($filename); }

function About() {  $GetCreditsStuff = new TemplateManager();  $GetCreditsStuff->Head_Foot(); }
function Powered() {  $GetCreditsStuff = new TemplateManager();  $GetCreditsStuff->Down_Foot(); }

?>