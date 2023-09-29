-- phpMyAdmin SQL Dump
-- version 3.3.9.2
-- http://www.phpmyadmin.net
--
-- Servidor: localhost
-- Tempo de Geração: Fev 27, 2023 as 04:58 PM
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
  `seo_link` varchar(120) NOT NULL DEFAULT 'news-article',
  `title` varchar(255) DEFAULT NULL,
  `category_id` int(10) unsigned NOT NULL DEFAULT '1',
  `shortstory` text,
  `longstory` text,
  `published` int(10) NOT NULL DEFAULT '0',
  `author` int(6) NOT NULL DEFAULT '1',
  `image` varchar(255) DEFAULT '/Public/Images/news/TS_Web60.png',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=2 ;

--
-- Extraindo dados da tabela `articles`
--

INSERT INTO `articles` (`id`, `seo_link`, `title`, `category_id`, `shortstory`, `longstory`, `published`, `author`, `image`) VALUES
(1, 'inserting-a-new-article', 'Inserting a new article', 2, '<p>Testing a new system for news system</p>', '<p>Hi %username% im souza and testing a new artile system for our hotel</p>', 1648607265, 1, 'images/news/AU_TS_HabboFilmAwards_v1.gif');

-- --------------------------------------------------------

--
-- Estrutura da tabela `articles_categories`
--

CREATE TABLE IF NOT EXISTS `articles_categories` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `caption` text NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=12 ;

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
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Extraindo dados da tabela `external_texts`
--

INSERT INTO `external_texts` (`skey`, `sval`) VALUES
('badge_desc_MDMWW', 'I won the Uber Home Exhibition competition!'),
('badge_name_ADM', 'Uber Staff'),
('badge_name_MDMWW', 'Home Exhibition'),
('badge_desc_ADM', 'I''m a staff member here, at Uber.'),
('badge_name_WHY', '...'),
('badge_desc_WHY', 'What?'),
('badge_name_Z63', 'BETA Lab Rat'),
('badge_desc_Z63', 'I helped test the new Uber.'),
('badge_name_BOT', 'Uber Bot'),
('badge_desc_BOT', 'I''m an automated worker at uberHotel.'),
('navigator.roomsettings.deleteroom.confirm.message', 'Are you sure you want to delete %room_name%? All the furniture (excluding wallpaper, floor, landscapes and stickie notes) will moved to "My Stuff".'),
('badge_name_HUG', 'Hug me'),
('badge_desc_HUG', '<3'),
('badge_name_US09', 'Uber Staff'),
('badge_desc_US09', 'I''m a staff member here, at Uber.'),
('handitem45', 'Excited Moodi'),
('handitem46', 'Happy Moodi'),
('infostand.button.savebranding', 'Save'),
('badge_name_ACH_BasicClub1', 'Uber Club member I'),
('badge_desc_ACH_BasicClub1', 'For joining the Uber Club.'),
('badge_name_ACH_BasicClub2', 'Uber Club member II'),
('badge_desc_ACH_BasicClub2', 'For %limit% months of Uber Club membership.'),
('badge_name_ACH_BasicClub3', 'Uber Club member III'),
('badge_desc_ACH_BasicClub3', 'For %limit% months of Uber Club membership.'),
('badge_name_ACH_BasicClub4', 'Uber Club member IV'),
('badge_desc_ACH_BasicClub4', 'For %limit% months of Uber Club membership.'),
('badge_name_ACH_BasicClub5', 'Uber Club member V'),
('badge_desc_ACH_BasicClub5', 'For %limit% months of Uber Club membership.'),
('catalog.page.club', 'Uber Club'),
('catalog.page.song_disk_shop', 'Music Discs'),
('catalog.purchase.price.credits_and_activitypoints.5', '%credits% Credit(s) + %activitypoints% Diamond(s)'),
('catalog.purchase.price.activitypoints.5', '%activitypoints% Diamonds'),
('catalog.alert.notenough.activitypoints.title.5', 'Not Enough Diamonds!'),
('catalog.purchase.youractivitypoints.5', 'Diamonds: %activitypoints%'),
('catalog.alert.notenough.activitypoints.description.5', 'Oops. It seems you don''t have enough Diamonds.'),
('catalog.purchase.confirmation.dialog.price.credits_and_activitypoints.5', '%credits% Credit(s) + %activitypoints% Diamond(s)'),
('catalog.purchase.confirmation.dialog.price.activitypoints.5', '%activitypoints% Diamond(s)'),
('catalog.purse.clubdays', 'HC: %months%m %days%d >>'),
('pet.command.0', 'Free'),
('pet.command.1', 'Sit'),
('pet.command.2', 'Down'),
('pet.command.3', 'Here'),
('pet.command.4', 'Beg'),
('pet.command.5', 'Play dead'),
('pet.command.6', 'Stay'),
('pet.command.7', 'Follow'),
('pet.command.8', 'Stand'),
('pet.command.9', 'Jump'),
('pet.command.10', 'Speak'),
('pet.command.11', 'Play'),
('pet.command.12', 'Silent'),
('pet.command.13', 'Nest'),
('pet.command.14', 'Drink'),
('pet.command.15', 'Follow left'),
('pet.command.16', 'Follow right'),
('pet.command.17', 'Play football'),
('pet.command.18', 'Come here'),
('pet.command.19', 'Bounce'),
('pet.command.20', 'Flat'),
('pet.command.21', 'Dance'),
('pet.command.22', 'Spin'),
('pet.command.23', 'Switch TV'),
('pet.command.24', 'Move forward'),
('pet.command.25', 'Turn left'),
('pet.command.26', 'Turn right'),
('pet.command.27', 'Relax'),
('pet.command.28', 'Croak'),
('pet.command.29', 'Dip'),
('pet.command.30', 'Wave'),
('pet.command.31', 'Mambo'),
('pet.command.32', 'High jump'),
('pet.command.33', 'Chicken dance'),
('pet.command.34', 'Triple jump'),
(' pet.command.35', 'Spread wings'),
(' pet.command.36', 'Breathe fire'),
('pet.command.37', 'Hang'),
('pet.command.38', 'Torch'),
('pet.command.40', 'Swing'),
('pet.command.41', 'Roll'),
('pet.command.42', 'Ring of fire'),
('pet.command.43', 'Eat'),
('pet.command.44', 'Wag Tail'),
('catalog.page.quest.new_user', 'Hello');

-- --------------------------------------------------------

--
-- Estrutura da tabela `external_variables`
--

CREATE TABLE IF NOT EXISTS `external_variables` (
  `skey` text NOT NULL,
  `sval` text NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

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
('logout.url', '%www%/me.php?pa=1'),
('moderatoractionlog.url', '%www%/manage/index.php?_cmd=actionlog&searchParam='),
('moderator.cmds', '[":alert x",":ban x",":kick x",":superban x",":shutup x",":unmute x",":transfer x",":softkick x"]'),
('interstitial.max.displays', '9999999'),
('interstitial.interval', '30'),
('interstitial.show.time', '3000'),
('client.hotel_view.image', 'hotel_view_images_hq/grass_view.png'),
('catalog.show.purse', 'false'),
('subscription.reminder.when.days.left', '5'),
('club.membership.extend.promotion.enabled', '0'),
('new.identity', '1'),
('avatar.widget.enabled', '1');

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
) ENGINE=InnoDB DEFAULT CHARSET=utf8 AUTO_INCREMENT=1 ;

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
) ENGINE=InnoDB DEFAULT CHARSET=utf8 AUTO_INCREMENT=1 ;

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
) ENGINE=InnoDB DEFAULT CHARSET=utf8 AUTO_INCREMENT=1 ;

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
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

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
  `account_password` text NOT NULL,
  `account_email` text NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=1 ;

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