<?php   
/*==========================================================================\
| RainbowCMS - A good CMS option for Snowlight Emulator                     |
| Copyright (C) 2021 - Created by LeoXDR edited by Souza                    |
| https://github.com/LeonSttKennedy/Snowlight-Emu                           |
| ========================================================================= |
| This CMS was developed with the permission of Meth0d                      |
\==========================================================================*/

	// Client Configuration
	
	// Connection information
	define('CLIENT_HOST', '192.168.15.72'); // Server IP
	define('CLIENT_PORT', 38101); // Server port
	
	// Swf path
	define('CLIENT_BASE', 'http://192.168.15.72/cdn.classichabbo.com/r38/gordon/RELEASE63-35255-34886-201108111108_ce2d130905ba279edbfb4208cd5035c0'); // Base
	define('CLIENT_SWFNAME', "Uber.swf");
	
	define('CLIENT_GAMEDATABASE', 'http://192.168.15.72/cdn.classichabbo.com/r38/gordon/RELEASE63-35255-34886-201108111108_ce2d130905ba279edbfb4208cd5035c0'); // Gamedata
	
	define('CLIENT_LOADING', 'Welcome to '.SITE_NAME.' BETA! Now loading.'); // Loading message
	define('CLIENT_CROSSDOMAIN', 1);
	define('CLIENT_SSOTICKET', 1);
	define('CLIENT_ORIGIN', "popup");
?>