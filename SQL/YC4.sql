-- =========================================================================
-- YC4.sql - Yeu cau 4: Backup va Recovery
-- =========================================================================

CONNECT BVOWNER/BVOwner#2026@localhost:1521/XEPDB1

CREATE OR REPLACE DIRECTORY BACKUP_DIR AS 'C:\benhvien_backup';
GRANT READ, WRITE ON DIRECTORY BACKUP_DIR TO SYSTEM;

-- Drop các bảng và procedure nếu có sẵn để tạo lại ở dưới
BEGIN
   EXECUTE IMMEDIATE 'DROP TABLE BVOWNER.BACKUP_LOG PURGE';
EXCEPTION
   WHEN OTHERS THEN
      IF SQLCODE != -942 THEN RAISE; END IF;
END;
/

BEGIN
   EXECUTE IMMEDIATE 'DROP PROCEDURE BVOWNER.PROC_AUTO_EXPORT';
EXCEPTION WHEN OTHERS THEN
   IF SQLCODE != -4043 THEN RAISE; END IF;
END;
/

BEGIN
   EXECUTE IMMEDIATE 'DROP PROCEDURE BVOWNER.PROC_IMPORT_RESTORE';
EXCEPTION WHEN OTHERS THEN
   IF SQLCODE != -4043 THEN RAISE; END IF;
END;
/


-- Bắt đầu tạo các bảng và procedure
BEGIN
   EXECUTE IMMEDIATE '
      CREATE TABLE BVOWNER.BACKUP_LOG (
         BAK_ID      NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
         BAK_TIME    TIMESTAMP DEFAULT SYSTIMESTAMP,
         BAK_TYPE    VARCHAR2(50),
         STATUS      VARCHAR2(20),
         FILE_NAME   VARCHAR2(100)
      )';
EXCEPTION
   WHEN OTHERS THEN IF SQLCODE != -955 THEN RAISE; END IF;
END;
/

CREATE OR REPLACE PROCEDURE PROC_AUTO_EXPORT AUTHID CURRENT_USER AS
   v_ts         TIMESTAMP;
   v_ts_text    VARCHAR2(30);
   v_job_handle NUMBER;
   v_job_name   VARCHAR2(50);
   v_dump_file  VARCHAR2(100);
   v_log_file   VARCHAR2(100);
   v_state      VARCHAR2(30);
BEGIN
   v_ts := SYSTIMESTAMP;
   v_ts_text := TO_CHAR(v_ts, 'YYYYMMDD_HH24MISS');
   v_job_name := 'EXP_BVOWNER_' || v_ts_text;
   v_dump_file := 'bvowner_' || v_ts_text || '.dmp';
   v_log_file := 'bvowner_' || v_ts_text || '.log';

   INSERT INTO BVOWNER.BACKUP_LOG (BAK_TIME, BAK_TYPE, STATUS, FILE_NAME)
   VALUES (v_ts, 'AUTO_EXPORT', 'STARTED', v_dump_file);
   COMMIT;

   v_job_handle := DBMS_DATAPUMP.OPEN('EXPORT', 'SCHEMA', NULL, v_job_name);
   DBMS_DATAPUMP.ADD_FILE(v_job_handle, v_dump_file, 'BACKUP_DIR');
   DBMS_DATAPUMP.ADD_FILE(v_job_handle, v_log_file, 'BACKUP_DIR');
   DBMS_DATAPUMP.START_JOB(v_job_handle);
   DBMS_DATAPUMP.WAIT_FOR_JOB(v_job_handle, v_state);

   UPDATE BVOWNER.BACKUP_LOG SET STATUS='DONE' WHERE BAK_TIME = v_ts;
   COMMIT;
EXCEPTION
   WHEN OTHERS THEN
      UPDATE BVOWNER.BACKUP_LOG SET STATUS='FAILED' WHERE BAK_TIME = v_ts;
      COMMIT;
      RAISE;
END;
/

-- Procedure PHUC HOI du lieu tu file dump
CREATE OR REPLACE PROCEDURE PROC_IMPORT_RESTORE(
    p_dump_file VARCHAR2
) AUTHID CURRENT_USER AS
    v_job_handle NUMBER;
    v_job_name   VARCHAR2(50);
    v_state      VARCHAR2(30);
BEGIN
    v_job_name := 'IMP_BVOWNER_' || TO_CHAR(SYSTIMESTAMP,'YYYYMMDD_HH24MISS');

    INSERT INTO BVOWNER.BACKUP_LOG (BAK_TIME, BAK_TYPE, STATUS)
    VALUES (SYSTIMESTAMP, 'IMPORT_RESTORE', 'STARTED');
    COMMIT;

    v_job_handle := DBMS_DATAPUMP.OPEN('IMPORT','SCHEMA',NULL,v_job_name);
    DBMS_DATAPUMP.ADD_FILE(v_job_handle, p_dump_file, 'BACKUP_DIR');
    -- Exclude backup log
    DBMS_DATAPUMP.METADATA_FILTER(v_job_handle,'NAME_EXPR','NOT IN (''BACKUP_LOG'')','TABLE');
    DBMS_DATAPUMP.SET_PARAMETER(v_job_handle, 'TABLE_EXISTS_ACTION', 'REPLACE');
    DBMS_DATAPUMP.START_JOB(v_job_handle);
    DBMS_DATAPUMP.WAIT_FOR_JOB(v_job_handle, v_state);

    UPDATE BVOWNER.BACKUP_LOG SET STATUS = 'DONE'
    WHERE BAK_TYPE = 'IMPORT_RESTORE' AND STATUS = 'STARTED';
    COMMIT;
EXCEPTION
    WHEN OTHERS THEN
        UPDATE BVOWNER.BACKUP_LOG SET STATUS = 'FAILED'
        WHERE BAK_TYPE = 'IMPORT_RESTORE' AND STATUS = 'STARTED';
        COMMIT;
        RAISE;
END;
/

BEGIN
   DBMS_SCHEDULER.DROP_JOB('BVOWNER.JOB_DAILY_BACKUP', TRUE);
EXCEPTION WHEN OTHERS THEN NULL;
END;
/

BEGIN
    DBMS_SCHEDULER.CREATE_JOB(
        job_name        => 'BVOWNER.JOB_DAILY_BACKUP',
        job_type        => 'STORED_PROCEDURE',
        job_action      => 'BVOWNER.PROC_AUTO_EXPORT',
        repeat_interval => 'FREQ=DAILY; BYHOUR=2; BYMINUTE=0; BYSECOND=0',
        start_date      => SYSTIMESTAMP,
        enabled         => TRUE,
        comments        => 'Tu dong sao luu BVOWNER moi ngay luc 2h sang'
    );
END;
/

PROMPT === YC4 DONE ===