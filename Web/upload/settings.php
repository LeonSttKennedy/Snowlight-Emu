<?php   
/*==========================================================================\
| RainbowCMS - A good CMS option for Snowlight Emulator                     |
| Copyright (C) 2021 - Created by LeoXDR edited by Souza                    |
| https://github.com/LeonSttKennedy/Snowlight-Emu                           |
| ========================================================================= |
| This CMS was developed with the permission of Meth0d                      |
\==========================================================================*/

// Requires all Classes and some Security Settings
require_once 'brain.php';

// Requires
// Actions
$PageAction = $_GET["pa"]; // Gets the Page Action

// Page Functions

// This function executes all actions from Website
if($PageAction == "1")
{
$friendreqs = $_POST['friendreqs'];
if($friendreqs == 'on')
$friendreqs = "1";
elseif($friendreqs == '')
$friendreqs = "0";

$mimic = $_POST['mimic'];
if($mimic == 'on')
$mimic = "1";
elseif($mimic == '')
$mimic = "0";

$gifts = $_POST['gifts'];
if($gifts == 'on')
$gifts = "1";
elseif($gifts == '')
$gifts = "0";
$query = mysql_query("UPDATE characters SET privacy_accept_friends = '".$friendreqs."', allow_mimic = '".$mimic."', allow_gifts = '".$gifts."' WHERE username = '".$_SESSION['account_name']."'");

$GetSecurity->Redirect("settings.php");
exit;
} 
else { }

// Lets Check is Sessions is Registred
$GetSecurity->Get_Session();

// Get CMS Headers and Stuffs

// Website Header
$GetTemplate->GetHeader();

// Content Header
$GetTemplate->GetContentHeader("User Settings");

$GetTemplate->WriteLine('<div id="main">');

// Getting the Page
$GetTemplate->GetTPL("settings");

// Getting Credits Stuff
$GetTemplate->GetContentFooter();

$GetTemplate->GetFooter();
?>
