<?php   
/*==========================================================================\
| RainbowCMS - A good CMS option for Snowlight Emulator                     |
| Copyright (C) 2021 - Created by LeoXDR edited by Souza                    |
| https://github.com/LeonSttKennedy/Snowlight-Emu                           |
| ========================================================================= |
| This CMS was developed with the permission of Meth0d                      |
\==========================================================================*/

require_once 'connection.class.php';

class Security extends MysqlConnection
{	
	public	function StartSession()
	{
		session_start();
	}
	
	public function Get_Session($session_status = FALSE)
	{
		if($_SESSION['login'] == $session_status)
		{
			$this->Redirect("login.php"); 
		} else {}
	}
	
	public function SessionStatus()
	{
		return $_SESSION['login'];
	}
	
	public function Logout($user)
	{
		MysqlConnection::DatabaseCleanUp();
		
		$_SESSION['login'] = FALSE; 
		session_destroy();
		$this->Redirect("index.php"); 
	}
	
	public function CreateSession($session_name, $session_value)
	{
		$CreatedSession = $_SESSION[''.$session_name.''] = "$session_value"; 
		return $CreatedSession;
	}
	
	public function Redirect($page)
	{
		header("Location: $page");
	}
	
	public function CheckFile($filename, $type)
	{
		$tpl_filepath = "requires/tpl";
		$texts_filepath = "requires/texts";
		
		switch($type)
		{
			case 00:
			if (file_exists(''.$tpl_filepath.'/'.$filename.'.tpl')) { return TRUE; } else {}
			break;
			
			case 11:
			if (file_exists(''.$texts_filepath.'/'.$filename.'.php')) { return TRUE; } else {}
			break;
			
		}
	}
	
	public function Reg($status)
	{
		switch($status)
		{
			case 1:
			header("Location: closed.php");
			break;
			
			case 0:
			return;
			break;
		}
	}
	
	public function Encrypt($mode, $value)
	{
		switch($mode)
		{
			case MD5: 
			$encoded_value = md5($value);
			return $encoded_value;
			break;
			
			case SHA1:
			$encoded_value = sha1($value);
			return $encoded_value;
			break;

			case BASE64:
			$encoded_value = base64_encode($value);
			return $encoded_value;
			break;
		}
	}
	
	public function IF_LogIN()
	{
		if($_SESSION['login'] == TRUE){	$this->Redirect("me.php"); } else {}
	}
}

?>