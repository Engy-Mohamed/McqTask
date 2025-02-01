<?php
if(! isset($_SESSION['logged']) or $_SESSION['logged']!=true)
	{
		header("location:login.php");
		die();
	}
$current_user_fullname = $_SESSION['fullname'];