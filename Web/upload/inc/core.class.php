<?php   
/*==========================================================================\
| RainbowCMS - A good CMS option for Snowlight Emulator                     |
| Copyright (C) 2021 - Created by LeoXDR edited by Souza                    |
| https://github.com/LeonSttKennedy/Snowlight-Emu                           |
| ========================================================================= |
| This CMS was developed with the permission of Meth0d                      |
\==========================================================================*/

require_once "website.config.php";

class Core
{
	public static function GetSystemStatus()
	{
		return intval(mysql_result(mysql_query("SELECT server_status FROM server_statistics LIMIT 1"), 0));
	}
	
	public function Mus($header, $data = '')
	{
		if (!MUS_ENABLED || $this->GetSystemStatus() == 0)
		{
			return;
		}
		
		$musData = $header . chr(1) . $data;

		$sock = @socket_create(AF_INET, SOCK_STREAM, getprotobyname('tcp'));
		@socket_connect($sock, MUS_HOST, intval(MUS_PORT));
		@socket_send($sock, $musData, strlen($musData), MSG_DONTROUTE);	
		@socket_close($sock);
	}
}
?>