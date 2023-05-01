/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;


-- chat 데이터베이스 구조 내보내기
CREATE DATABASE IF NOT EXISTS `chat` /*!40100 DEFAULT CHARACTER SET utf8mb4 */;
USE `chat`;

-- 테이블 chat.accounts 구조 내보내기
CREATE TABLE IF NOT EXISTS `accounts` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `username` varchar(32) NOT NULL DEFAULT '',
  `password` varchar(64) NOT NULL DEFAULT '',
  `name` varchar(16) DEFAULT NULL,
  `registered_mac` varchar(16) NOT NULL DEFAULT '',
  `admin` tinyint(1) unsigned NOT NULL DEFAULT 0,
  `message` varchar(20) NOT NULL DEFAULT '',
  `avatar` mediumblob DEFAULT NULL,
  `avatar_update_date` bigint(20) unsigned NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`),
  UNIQUE KEY `인덱스 2` (`username`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4;

-- 내보낼 데이터가 선택되어 있지 않습니다.

-- 테이블 chat.channels 구조 내보내기
CREATE TABLE IF NOT EXISTS `channels` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(20) NOT NULL DEFAULT '0',
  `is_secret` tinyint(1) unsigned NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=29 DEFAULT CHARSET=utf8mb4;

-- 내보낼 데이터가 선택되어 있지 않습니다.

-- 테이블 chat.channel_users 구조 내보내기
CREATE TABLE IF NOT EXISTS `channel_users` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `channel_id` int(10) unsigned NOT NULL,
  `user_id` int(10) unsigned NOT NULL,
  `is_admin` tinyint(1) unsigned NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`),
  KEY `FK__accounts` (`user_id`),
  KEY `인덱스 2` (`channel_id`) USING BTREE,
  CONSTRAINT `FK__accounts` FOREIGN KEY (`user_id`) REFERENCES `accounts` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK__channels` FOREIGN KEY (`channel_id`) REFERENCES `channels` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=57 DEFAULT CHARSET=utf8mb4;

-- 내보낼 데이터가 선택되어 있지 않습니다.

-- 테이블 chat.friends 구조 내보내기
CREATE TABLE IF NOT EXISTS `friends` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `user_id` int(10) unsigned NOT NULL,
  `friend_user_id` int(10) unsigned NOT NULL,
  PRIMARY KEY (`id`),
  KEY `FK_friends_accounts_2` (`friend_user_id`),
  KEY `인덱스 2` (`user_id`) USING BTREE,
  CONSTRAINT `FK_friends_accounts` FOREIGN KEY (`user_id`) REFERENCES `accounts` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_friends_accounts_2` FOREIGN KEY (`friend_user_id`) REFERENCES `accounts` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4;

-- 내보낼 데이터가 선택되어 있지 않습니다.

-- 프로시저 chat.getRelatedUsers 구조 내보내기
DELIMITER //
CREATE PROCEDURE `getRelatedUsers`(
	IN `userId` INT
)
    READS SQL DATA
    SQL SECURITY INVOKER
BEGIN
    SELECT DISTINCT a.id, a.username, a.name, a.message
    FROM accounts a
    INNER JOIN (
        SELECT cu.user_id
        FROM channel_users cu
        WHERE cu.channel_id IN (
            SELECT cu2.channel_id
            FROM channel_users cu2
            WHERE cu2.user_id = userId
        )
        UNION
        SELECT f.friend_user_id
        FROM friends f
        WHERE f.user_id = userId
        UNION
        SELECT userId
    ) AS related ON a.id = related.user_id
    ORDER BY a.id;
END//
DELIMITER ;

-- 테이블 chat.messages 구조 내보내기
CREATE TABLE IF NOT EXISTS `messages` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `channel_id` int(10) unsigned NOT NULL,
  `user_id` int(10) unsigned NOT NULL,
  `date` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  `message` blob DEFAULT NULL,
  `attachment` longblob DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `FK_messages_accounts` (`user_id`),
  KEY `인덱스 2` (`channel_id`) USING BTREE,
  CONSTRAINT `FK_messages_accounts` FOREIGN KEY (`user_id`) REFERENCES `accounts` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_messages_channels` FOREIGN KEY (`channel_id`) REFERENCES `channels` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=36 DEFAULT CHARSET=utf8mb4;

-- 내보낼 데이터가 선택되어 있지 않습니다.

/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
