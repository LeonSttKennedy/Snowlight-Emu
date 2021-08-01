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
$GetSecurity->IF_LogIN();

// Gets the Page Action
$PageAction = $_GET["pa"];

// Functions of The Page
// This function executes all actions from Website
if($PageAction == "1")
{
	$GetUsers->Register($_POST["email"], $_POST["username"], $_POST["password"], $_POST["password2"]);
}
else {}

// Registers are Enabled ?
$GetSecurity->Reg(REG_STATUS);

// Get CMS Header Static
$GetTemplate->GetHeader(); // Website Header
$GetTemplate->GetContentHeader("Register"); // Content Header
$GetTemplate->WriteLine('<div id="main">');

// Getting Messages Session
$GetTemplate->GetSession(1);

// Getting the Page
$GetTemplate->GetTPL('register');
$GetTemplate->WriteLineSeveral('</div>', 2);

// Getting Credits Stuff
$GetTemplate->GetContentFooter();
$GetTemplate->GetFooter();
?>
