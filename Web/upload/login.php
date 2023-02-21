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

define("CURRENT_PAGE", "login");

// Requires
$GetSecurity->IF_LogIN();

// Gets the Page Action
$PageAction = $_GET["pa"];

// Functions of The Page
// This function executes all actions from Website
if($PageAction == "1") { $GetUsers->TryLogin($_POST["email"], $_POST["password"]); } 
// User´s Registy
else{}

// Get CMS Header Static
$GetTemplate->GetHeader(); // Website Header
$GetTemplate->GetContentHeader("Login"); // Content Header
$GetTemplate->WriteLine('<div id="main">');

// Getting Messages Session
$GetTemplate->GetSession(2);

// Getting the Page
$GetTemplate->GetTPL("login");
$GetTemplate->WriteLine('</div>');

// Credit Stuff
$GetTemplate->GetContentFooter();
$GetTemplate->GetFooter();
?>
