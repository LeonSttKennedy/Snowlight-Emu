-- phpMyAdmin SQL Dump
-- version 3.3.9.2
-- http://www.phpmyadmin.net
--
-- Servidor: localhost
-- Tempo de Geração: Ago 01, 2021 as 02:34 PM
-- Versão do Servidor: 5.5.10
-- Versão do PHP: 5.3.6

SET SQL_MODE="NO_AUTO_VALUE_ON_ZERO";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;

--
-- Banco de Dados: `snowdb`
--

-- --------------------------------------------------------

--
-- Estrutura da tabela `articles`
--

CREATE TABLE IF NOT EXISTS `articles` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `seo_link` varchar(120) CHARACTER SET utf8 NOT NULL DEFAULT 'news-article',
  `title` varchar(255) COLLATE latin1_general_ci DEFAULT NULL,
  `category_id` int(10) unsigned NOT NULL DEFAULT '1',
  `shortstory` text COLLATE latin1_general_ci,
  `longstory` text COLLATE latin1_general_ci,
  `published` int(10) NOT NULL DEFAULT '0',
  `author` int(6) NOT NULL DEFAULT '1',
  `image` varchar(255) COLLATE latin1_general_ci DEFAULT '/Public/Images/news/TS_Web60.png',
  PRIMARY KEY (`id`)
) ENGINE=MyISAM  DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci AUTO_INCREMENT=1 ;

--
-- Extraindo dados da tabela `articles`
--

-- --------------------------------------------------------

--
-- Estrutura da tabela `articles_categories`
--

CREATE TABLE IF NOT EXISTS `articles_categories` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `caption` text NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=MyISAM  DEFAULT CHARSET=latin1 AUTO_INCREMENT=12 ;

--
-- Extraindo dados da tabela `articles_categories`
--

INSERT INTO `articles_categories` (`id`, `caption`) VALUES
(0, 'Uncategorized'),
(1, 'Uber'),
(2, 'Technical'),
(3, 'Updates'),
(4, 'Competitions'),
(5, 'Polls'),
(6, 'Sponsored'),
(7, 'Credits'),
(8, 'Uber Club'),
(9, 'VIP'),
(10, 'Furni'),
(11, 'Support');

-- --------------------------------------------------------

--
-- Estrutura da tabela `external_texts`
--

CREATE TABLE IF NOT EXISTS `external_texts` (
  `skey` text NOT NULL,
  `sval` text NOT NULL
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

--
-- Extraindo dados da tabela `external_texts`
--

INSERT INTO `external_texts` (`skey`, `sval`) VALUES
('badge_desc_MDMWW', 'I won the Uber Home Exhibition competition!'),
('badge_name_ADM', 'Uber Staff'),
('badge_name_MDMWW', 'Home Exhibition'),
('badge_desc_ADM', 'I''m a staff member here, at Uber.'),
('badge_name_CAA', 'Trial Moderator'),
('badge_desc_CAA', 'I am a new Moderator being trialed at Uber.'),
('badge_name_WHY', '...'),
('badge_desc_WHY', 'What?'),
('badge_name_Z63', 'BETA Lab Rat'),
('badge_desc_Z63', 'I helped test the new Uber.'),
('badge_name_BOT', 'Uber Bot'),
('badge_desc_BOT', 'I''m an automated worker at uberHotel.'),
('navigator.roomsettings.deleteroom.confirm.message', 'Are you sure you want to delete %room_name%? All the furniture (excluding wallpaper, floor, landscapes and stickie notes) will moved to "My Stuff".'),
('badge_name_HUG', 'Hug me'),
('badge_desc_HUG', '<3'),
('badge_name_HC1', 'Uber Club Member'),
('badge_desc_HC1', 'A member of the exclusive Uber Club.');

-- --------------------------------------------------------

--
-- Estrutura da tabela `external_variables`
--

CREATE TABLE IF NOT EXISTS `external_variables` (
  `skey` text NOT NULL,
  `sval` text NOT NULL
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

--
-- Extraindo dados da tabela `external_variables`
--

INSERT INTO `external_variables` (`skey`, `sval`) VALUES
('client.fatal.error.url', '%www%/account/disconnected?source=client_error'),
('habboinfotool.url', '%www%/manage/index.php?_cmd=userinfo&searchParam='),
('wordfilter.url', '%www%/manage/index.php?_cmd=wordfilter'),
('roomadmin.url', '%www%/manage/index.php?_cmd=roomadmin&searchParam='),
('link.format.habboclub', '%www%/credits/uberclub'),
('logout.disconnect.url', '%www%/account/disconnected?reason=disconnected&origin=%origin%'),
('logout.url', '%www%/account/logout'),
('moderatoractionlog.url', '%www%/manage/index.php?_cmd=actionlog&searchParam='),
('moderator.cmds', '[":alert x",":ban x",":kick x",":superban x",":shutup x",":unmute x",":transfer x",":softkick x"]'),
('interstitial.max.displays', '9999999'),
('interstitial.interval', '30'),
('interstitial.show.time', '3000'),
('client.hotel_view.image', 'hotel_view_images_hq/grass_view.png');

-- --------------------------------------------------------

--
-- Estrutura da tabela `moderation_forum_replies`
--

CREATE TABLE IF NOT EXISTS `moderation_forum_replies` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `thread_id` int(10) unsigned NOT NULL,
  `poster` varchar(120) NOT NULL,
  `date` varchar(50) NOT NULL,
  `message` text NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=MyISAM  DEFAULT CHARSET=latin1 AUTO_INCREMENT=3 ;

--
-- Extraindo dados da tabela `moderation_forum_replies`
--


-- --------------------------------------------------------

--
-- Estrutura da tabela `moderation_forum_threads`
--

CREATE TABLE IF NOT EXISTS `moderation_forum_threads` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `poster` varchar(100) NOT NULL,
  `subject` varchar(120) NOT NULL,
  `date` varchar(50) NOT NULL,
  `timestamp` double NOT NULL,
  `message` text NOT NULL,
  `locked` enum('0','1') NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=MyISAM  DEFAULT CHARSET=latin1 AUTO_INCREMENT=3 ;

--
-- Extraindo dados da tabela `moderation_forum_threads`
--

-- --------------------------------------------------------

--
-- Estrutura da tabela `notes`
--

CREATE TABLE IF NOT EXISTS `notes` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `content` varchar(500) NOT NULL,
  `date` varchar(100) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=MyISAM  DEFAULT CHARSET=latin1 AUTO_INCREMENT=2 ;

--
-- Extraindo dados da tabela `notes`
--


-- --------------------------------------------------------

--
-- Estrutura da tabela `site_config`
--

CREATE TABLE IF NOT EXISTS `site_config` (
  `maintenance` enum('0','1') NOT NULL DEFAULT '0',
  `maintenance_text` text NOT NULL,
  `reg_status` enum('0','1') NOT NULL DEFAULT '0'
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

--
-- Extraindo dados da tabela `site_config`
--

INSERT INTO `site_config` (`maintenance`, `maintenance_text`, `reg_status`) VALUES
('0', 'We are patching some bugs founded in our hotel.', '0');

-- --------------------------------------------------------

--
-- Estrutura da tabela `users`
--

CREATE TABLE IF NOT EXISTS `users` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `account_name` varchar(400) NOT NULL,
  `account_password` text NOT NULL,
  `account_email` text NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=1 ;

--
-- Extraindo dados da tabela `users`
--


INSERT INTO `rights` (`id`, `set_id`, `right_id`) VALUES
(NULL, 100, 'fuse_ignore_maintenance'),
(NULL, 100, 'hk_login'),
(NULL, 100, 'hk_moderation'),
(NULL, 100, 'hk_sitemanagement'),
(NULL, 100, 'hk_catalog'),
(NULL, 200, 'hk_external_login');