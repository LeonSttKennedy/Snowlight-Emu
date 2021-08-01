<?php
/*==========================================================================\
| RainbowCMS - A good CMS option for Snowlight Emulator                     |
| Copyright (C) 2021 - Created by LeoXDR edited by Souza                    |
| https://github.com/LeonSttKennedy/Snowlight-Emu                           |
| ========================================================================= |
| This CMS was developed with the permission of Meth0d                      |
\==========================================================================*/


// Initialize Session Messages
if($_SESSION['error']=="1"){
echo '<p><div class="attention"><center><br>This username is prohibited, please choose another username.<br><br></center></div><p>
';
}
else if($_SESSION['error']=="2"){
echo '<p><div class="attention"><center><br>There are blank fields, please review the form.<br><br></center></div><p>
';
}
else if($_SESSION['error']=="3"){
echo '<p><div class="attention"><center><br>Name already registered, please choose another username.<br><br></center></div><p>
';
}
else if($_SESSION['error']=="4"){
echo '<p><div class="attention"><center><br>Passwords don\'t match.<br><br></center></div><p>
';
}
else if($_SESSION['error']=="5"){
echo '<p><div class="attention"><center><br>Email already registered, please choose another email.<br><br></center></div><p>
';
}

// Destroy Error Session
unset($_SESSION['error']);
?>