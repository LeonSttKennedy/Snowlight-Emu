<?php
/*==========================================================================\
| RainbowCMS - A good CMS option for Snowlight Emulator                     |
| Copyright (C) 2021 - Created by LeoXDR edited by Souza                    |
| https://github.com/LeonSttKennedy/Snowlight-Emu                           |
| ========================================================================= |
| This CMS was developed with the permission of Meth0d                      |
\==========================================================================*/


// Initialize Session Messages
if($_SESSION['login_error']=="1"){
echo '<p><div class="attention"><center><br>There are blank fields, please review the form.<br><br></center></div><p>
';}
else if($_SESSION['login_error']=="2"){
echo '<p><div class="attention"><center><br>Invalid email or password. Please try again.<br><br></center></div><p>
';}

// Destroy Error Session
unset($_SESSION['login_error']);
?>