-- =========================================================================
-- YC1.sql - Yeu cau 1: TC#1..TC#5 (khong goi file ngoai)
-- =========================================================================

CONNECT BVOWNER/BVOwner#2026@localhost:1521/XEPDB1

-- TC#1 - Tao users
--DECLARE
--    v_sql  VARCHAR2(4000);
--    v_pw   VARCHAR2(50);
--    CURSOR c_users IS
--        SELECT ORA_USERNAME uname 
--        FROM BVOWNER.NHANVIEN 
--        WHERE ORA_USERNAME IS NOT NULL
--    
--        UNION
--    
--        SELECT ORA_USERNAME
--        FROM (
--            SELECT ORA_USERNAME
--            FROM BVOWNER.BENHNHAN
--            WHERE ORA_USERNAME IS NOT NULL
--            ORDER BY MABN
--        )
--        WHERE ROWNUM <= 10;
--BEGIN
--    FOR r IN c_users LOOP
--        IF REGEXP_LIKE(r.uname, '^DPV_') THEN v_pw := 'DPV@bv2026!';
--        ELSIF REGEXP_LIKE(r.uname, '^BS_') THEN v_pw := 'BS@bv2026!';
--        ELSIF REGEXP_LIKE(r.uname, '^KTV_') THEN v_pw := 'KTV@bv2026!';
--        ELSIF REGEXP_LIKE(r.uname, '^BN_') THEN v_pw := 'BN@bv2026!';
--        ELSE v_pw := 'User@bv2026!';
--        END IF;
--
--        v_sql := 'CREATE USER ' || r.uname || ' IDENTIFIED BY "' || v_pw || '" DEFAULT TABLESPACE USERS QUOTA 0 ON USERS';
--        BEGIN
--            EXECUTE IMMEDIATE v_sql;
--        EXCEPTION
--            WHEN OTHERS THEN IF SQLCODE != -1920 THEN RAISE; END IF;
--        END;
--    END LOOP;
--END;


-- Procedure tạo tất cả user theo từng đợt (mỗi lần 1000 user)
CREATE OR REPLACE PROCEDURE CREATE_USERS_BATCH (
    p_start NUMBER,
    p_end   NUMBER
)
AS
    v_sql    VARCHAR2(4000);
    v_pw     VARCHAR2(50);
    v_count  NUMBER := 0;
BEGIN
    FOR r IN (
        SELECT ORA_USERNAME
        FROM (
            SELECT ORA_USERNAME,
                   ROW_NUMBER() OVER (ORDER BY PRIORITY, SORT_KEY) rn
            FROM (
                SELECT ORA_USERNAME, 1 AS PRIORITY, MANV AS SORT_KEY
                FROM BVOWNER.NHANVIEN
                WHERE ORA_USERNAME IS NOT NULL

                UNION ALL

                SELECT ORA_USERNAME, 2 AS PRIORITY, MABN AS SORT_KEY
                FROM BVOWNER.BENHNHAN
                WHERE ORA_USERNAME IS NOT NULL
            )
        )
        WHERE rn BETWEEN p_start AND p_end
    )
    LOOP
        BEGIN
            -- password theo loại user
            IF REGEXP_LIKE(r.ORA_USERNAME, '^DPV_') THEN
                v_pw := 'DPV@bv2026!';
            ELSIF REGEXP_LIKE(r.ORA_USERNAME, '^BS_') THEN
                v_pw := 'BS@bv2026!';
            ELSIF REGEXP_LIKE(r.ORA_USERNAME, '^KTV_') THEN
                v_pw := 'KTV@bv2026!';
            ELSIF REGEXP_LIKE(r.ORA_USERNAME, '^BN_') THEN
                v_pw := 'BN@bv2026!';
            ELSE
                v_pw := 'User@bv2026!';
            END IF;

            v_sql :=
                'CREATE USER ' || r.ORA_USERNAME ||
                ' IDENTIFIED BY "' || v_pw || '" ' ||
                'DEFAULT TABLESPACE USERS ' ||
                'QUOTA 0 ON USERS';

            EXECUTE IMMEDIATE v_sql;

            v_count := v_count + 1;

            IF MOD(v_count,1000)=0 THEN
                DBMS_OUTPUT.PUT_LINE(
                    v_count || ' users created...'
                );
            END IF;

        EXCEPTION
            WHEN OTHERS THEN
                -- ORA-01920: user exists
                IF SQLCODE != -1920 THEN
                    DBMS_OUTPUT.PUT_LINE(
                        r.ORA_USERNAME || ' -> ' || SQLERRM
                    );
                END IF;
        END;

    END LOOP;

    DBMS_OUTPUT.PUT_LINE('DONE: ' || v_count || ' users created.');

END;

/

-- Gọi procedure tạo user theo từng đợt 1000 user
DECLARE
    v_start NUMBER := 82109;
    v_batch NUMBER := 5000;
    v_end   NUMBER;
    v_max   NUMBER := 100170;
BEGIN

    WHILE v_start <= v_max LOOP

        v_end := LEAST(v_start + v_batch - 1, v_max);

        CREATE_USERS_BATCH(v_start, v_end);

        v_start := v_end + 1;

    END LOOP;

END;
/

CONNECT BVOWNER/BVOwner#2026@localhost:1521/XEPDB1
CREATE OR REPLACE VIEW V_CURRENT_BENHNHAN AS
    SELECT * FROM BENHNHAN
    WHERE ORA_USERNAME = SYS_CONTEXT('USERENV','SESSION_USER'); 
    
CREATE OR REPLACE VIEW V_CURRENT_NHANVIEN AS
    SELECT * FROM NHANVIEN
    WHERE ORA_USERNAME = SYS_CONTEXT('USERENV','SESSION_USER');


CREATE OR REPLACE VIEW V_CURRENT_HSBA_DV_KTV AS
    SELECT D.*
    FROM HSBA_DV D
    WHERE D.MAKTV = (
        SELECT N.MANV
        FROM NHANVIEN N
        WHERE N.ORA_USERNAME = SYS_CONTEXT('USERENV','SESSION_USER')
          AND ROWNUM = 1
    );


CREATE OR REPLACE TRIGGER TRG_UPDATE_BENHNHAN_VIEW
INSTEAD OF UPDATE ON V_CURRENT_BENHNHAN
FOR EACH ROW
BEGIN
    UPDATE BENHNHAN
    SET
        SONHA        = :NEW.SONHA,
        TENDUONG     = :NEW.TENDUONG,
        QUANHUYEN    = :NEW.QUANHUYEN,
        TINHTP       = :NEW.TINHTP,
        TIENSUBENH   = :NEW.TIENSUBENH,
        TIENSUBENHGD = :NEW.TIENSUBENHGD,
        DIUNGTUOC    = :NEW.DIUNGTUOC
    WHERE MABN = :OLD.MABN
      AND ORA_USERNAME = SYS_CONTEXT('USERENV','SESSION_USER');
END;
/

CREATE OR REPLACE TRIGGER TRG_UPDATE_NHANVIEN_VIEW
INSTEAD OF UPDATE ON V_CURRENT_NHANVIEN_UPDATABLE
FOR EACH ROW
BEGIN
    UPDATE NHANVIEN
    SET
        QUEQUAN  = :NEW.QUEQUAN,
        SODT     = :NEW.SODT
    WHERE MANV = :OLD.MANV
      AND ORA_USERNAME = SYS_CONTEXT('USERENV','SESSION_USER');
END;
/

-- TC#2..TC#5 - RBAC + VPD
CONNECT BVOWNER/BVOwner#2026@localhost:1521/XEPDB1

BEGIN EXECUTE IMMEDIATE 'CREATE ROLE ROLE_DIEUPHOI'; EXCEPTION WHEN OTHERS THEN IF SQLCODE != -1921 THEN RAISE; END IF; END;
/
BEGIN EXECUTE IMMEDIATE 'CREATE ROLE ROLE_BACSI'; EXCEPTION WHEN OTHERS THEN IF SQLCODE != -1921 THEN RAISE; END IF; END;
/
BEGIN EXECUTE IMMEDIATE 'CREATE ROLE ROLE_KTV'; EXCEPTION WHEN OTHERS THEN IF SQLCODE != -1921 THEN RAISE; END IF; END;
/
BEGIN EXECUTE IMMEDIATE 'CREATE ROLE ROLE_BENHNHAN'; EXCEPTION WHEN OTHERS THEN IF SQLCODE != -1921 THEN RAISE; END IF; END;
/

GRANT CREATE SESSION TO ROLE_DIEUPHOI, ROLE_BACSI, ROLE_KTV, ROLE_BENHNHAN;

GRANT SELECT, INSERT ON BVOWNER.BENHNHAN TO ROLE_DIEUPHOI;
GRANT UPDATE (TENBN,PHAI,NGAYSINH,CCCD,SONHA,TENDUONG,QUANHUYEN,TINHTP,TIENSUBENH,TIENSUBENHGD,DIUNGTUOC) ON BVOWNER.BENHNHAN TO ROLE_DIEUPHOI;
GRANT INSERT ON BVOWNER.HSBA TO ROLE_DIEUPHOI;
GRANT UPDATE (MAKHOA, MABS) ON BVOWNER.HSBA TO ROLE_DIEUPHOI;
GRANT UPDATE (MAKTV) ON BVOWNER.HSBA_DV TO ROLE_DIEUPHOI;
GRANT SELECT ON BVOWNER.HSBA TO ROLE_DIEUPHOI;
GRANT SELECT ON BVOWNER.HSBA_DV TO ROLE_DIEUPHOI;
GRANT SELECT ON BVOWNER.V_CURRENT_NHANVIEN TO ROLE_DIEUPHOI;

GRANT SELECT ON BVOWNER.HSBA TO ROLE_BACSI;
GRANT INSERT, DELETE ON BVOWNER.HSBA_DV TO ROLE_BACSI;
GRANT SELECT ON BVOWNER.HSBA_DV TO ROLE_BACSI;
GRANT UPDATE (CHANDOAN, DIEUTRI, KETLUAN) ON BVOWNER.HSBA TO ROLE_BACSI;
GRANT SELECT ON BVOWNER.BENHNHAN TO ROLE_BACSI;
GRANT UPDATE (TIENSUBENH, TIENSUBENHGD, DIUNGTUOC) ON BVOWNER.BENHNHAN TO ROLE_BACSI;
GRANT SELECT, INSERT, DELETE, UPDATE ON BVOWNER.DONTHUOC TO ROLE_BACSI;
GRANT SELECT ON BVOWNER.V_CURRENT_NHANVIEN TO ROLE_BACSI;

BEGIN EXECUTE IMMEDIATE 'REVOKE SELECT, UPDATE ON BVOWNER.HSBA_DV FROM ROLE_KTV'; EXCEPTION WHEN OTHERS THEN NULL; END;
/
GRANT SELECT ON BVOWNER.V_CURRENT_NHANVIEN TO ROLE_KTV;
GRANT SELECT, UPDATE (KETQUA) ON BVOWNER.V_CURRENT_HSBA_DV_KTV TO ROLE_KTV;
GRANT SELECT, UPDATE ON BVOWNER.V_CURRENT_BENHNHAN TO ROLE_BENHNHAN;
GRANT SELECT, UPDATE ON BVOWNER.V_CURRENT_NHANVIEN_UPDATABLE TO ROLE_DIEUPHOI, ROLE_BACSI, ROLE_KTV;

-- phân quyền cho các role
GRANT SELECT ON BVOWNER.THONGBAO TO ROLE_DIEUPHOI, ROLE_BACSI, ROLE_KTV;

--GRANT ROLE_DIEUPHOI TO DPV_001,DPV_002,DPV_003,DPV_004,DPV_005,DPV_006,DPV_007,DPV_008,DPV_009,DPV_010,
--                       DPV_011,DPV_012,DPV_013,DPV_014,DPV_015,DPV_016,DPV_017,DPV_018,DPV_019,DPV_020;
--GRANT ROLE_BACSI TO BS_001,BS_002,BS_003,BS_004,BS_005,BS_006,BS_007,BS_008,BS_009,BS_010;
--GRANT ROLE_KTV TO KTV_001,KTV_002,KTV_003,KTV_004,KTV_005;
--GRANT ROLE_BENHNHAN TO BN_000001,BN_000002,BN_000003,BN_000004,BN_000005,BN_000006,BN_000007,BN_000008,BN_000009,BN_000010;

-- Procedure gán quyền cho tất cả các nhân viên và bệnh nhân
CREATE OR REPLACE PROCEDURE GRANT_ROLES_BATCH (
    p_start NUMBER,
    p_end   NUMBER
)
AS
    v_role   VARCHAR2(50);
    v_count  NUMBER := 0;
BEGIN
    FOR r IN (
        SELECT ORA_USERNAME
        FROM (
            SELECT ORA_USERNAME,
                   ROW_NUMBER() OVER (
                       ORDER BY priority, sort_key
                   ) rn
            FROM (
                SELECT ORA_USERNAME,
                       1 AS priority,
                       MANV AS sort_key
                FROM BVOWNER.NHANVIEN
                WHERE ORA_USERNAME IS NOT NULL

                UNION ALL

                SELECT ORA_USERNAME,
                       2 AS priority,
                       MABN AS sort_key
                FROM BVOWNER.BENHNHAN
                WHERE ORA_USERNAME IS NOT NULL
            )
        )
        WHERE rn BETWEEN p_start AND p_end
    )
    LOOP
        BEGIN
            IF r.ORA_USERNAME LIKE 'DPV_%' THEN
                v_role := 'ROLE_DIEUPHOI';
            ELSIF r.ORA_USERNAME LIKE 'BS_%' THEN
                v_role := 'ROLE_BACSI';
            ELSIF r.ORA_USERNAME LIKE 'KTV_%' THEN
                v_role := 'ROLE_KTV';
            ELSIF r.ORA_USERNAME LIKE 'BN_%' THEN
                v_role := 'ROLE_BENHNHAN';
            ELSE
                v_role := NULL;
            END IF;
            
            IF v_role IS NOT NULL THEN
                EXECUTE IMMEDIATE
                    'GRANT ' || v_role ||
                    ' TO ' || r.ORA_USERNAME;
                v_count := v_count + 1;
                IF MOD(v_count,1000)=0 THEN
                    DBMS_OUTPUT.PUT_LINE(
                        v_count || ' role grants completed...'
                    );
                END IF;

            END IF;
        EXCEPTION
            WHEN OTHERS THEN
                DBMS_OUTPUT.PUT_LINE(
                    r.ORA_USERNAME || ' -> ' || SQLERRM
                );
        END;
    END LOOP;
END;

/

-- Gọi procedure gán quyền cho tất cả user theo từng đợt
DECLARE
    v_start NUMBER := 1;
    v_batch NUMBER := 5000;
    v_end   NUMBER;
    v_max   NUMBER := 100170;
BEGIN
    WHILE v_start <= v_max LOOP
        v_end := LEAST(
            v_start + v_batch - 1,
            v_max
        );
        GRANT_ROLES_BATCH(v_start, v_end);
        v_start := v_end + 1;
    END LOOP;
END;

/

CONNECT BVOWNER/BVOwner#2026@localhost:1521/XEPDB1

BEGIN DBMS_RLS.DROP_POLICY('BVOWNER','HSBA','POLICY_HSBA_VPD'); EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN DBMS_RLS.DROP_POLICY('BVOWNER','HSBA','POL_HSBA_ACCESS'); EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN DBMS_RLS.DROP_POLICY('BVOWNER','BENHNHAN','POLICY_BN_VPD'); EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN DBMS_RLS.DROP_POLICY('BVOWNER','BENHNHAN','POL_BN_BACSI'); EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN DBMS_RLS.DROP_POLICY('BVOWNER','DONTHUOC','POL_DT_BACSI'); EXCEPTION WHEN OTHERS THEN NULL; END;
/

CREATE OR REPLACE FUNCTION pol_hsba_func (
    p_schema IN VARCHAR2,
    p_obj    IN VARCHAR2
) RETURN VARCHAR2 
AS
    v_user VARCHAR2(100);
    v_manv VARCHAR2(20);
BEGIN
    -- Lấy tên user đang đăng nhập
    v_user := SYS_CONTEXT('USERENV', 'SESSION_USER');

    -- Điều phối viên: xem tất cả
    IF DBMS_SESSION.IS_ROLE_ENABLED('ROLE_DIEUPHOI') OR v_user LIKE 'DPV\_%' ESCAPE '\' THEN
        RETURN '1=1';
    END IF;

    -- Y sĩ/Bác sĩ: chỉ xem HSBA do mình phụ trách
    IF DBMS_SESSION.IS_ROLE_ENABLED('ROLE_BACSI') OR v_user LIKE 'BS\_%' ESCAPE '\' THEN
        BEGIN
            SELECT MANV
            INTO v_manv
            FROM BVOWNER.NHANVIEN
            WHERE ORA_USERNAME = v_user
              AND ROWNUM = 1;
        EXCEPTION
            WHEN NO_DATA_FOUND THEN
                RETURN '1=2';
        END;

        -- HSBA.MABS lưu MANV (ví dụ: NV_BS_001)
        RETURN 'MABS = ''' || v_manv || '''';
    END IF;

    -- Các trường hợp khác không được xem
    RETURN '1=2';
END;
/

CREATE OR REPLACE FUNCTION pol_bn_func (
    p_schema IN VARCHAR2,
    p_obj    IN VARCHAR2
) RETURN VARCHAR2 
AS
    v_user VARCHAR2(100);
    v_manv VARCHAR2(20);
BEGIN
    v_user := SYS_CONTEXT('USERENV', 'SESSION_USER');

    -- Điều phối viên: xem tất cả
    IF DBMS_SESSION.IS_ROLE_ENABLED('ROLE_DIEUPHOI') OR v_user LIKE 'DPV\_%' ESCAPE '\' THEN
        RETURN '1=1';
    END IF;

    -- Y sĩ/Bác sĩ: chỉ thấy bệnh nhân có HSBA do mình phụ trách
    IF DBMS_SESSION.IS_ROLE_ENABLED('ROLE_BACSI') OR v_user LIKE 'BS\_%' ESCAPE '\' THEN
        BEGIN
            SELECT MANV
            INTO v_manv
            FROM BVOWNER.NHANVIEN
            WHERE ORA_USERNAME = v_user
              AND ROWNUM = 1;
        EXCEPTION
            WHEN NO_DATA_FOUND THEN
                RETURN '1=2';
        END;

        RETURN 'MABN IN (SELECT H.MABN FROM BVOWNER.HSBA H WHERE H.MABS = ''' || v_manv || ''')';
    END IF;

    -- Các trường hợp khác không được xem
    RETURN '1=2';
END;
/

-- Áp dụng cho bảng HSBA
BEGIN
    DBMS_RLS.ADD_POLICY(
        object_schema   => 'BVOWNER',
        object_name     => 'HSBA',
        policy_name     => 'POLICY_HSBA_VPD',
        function_schema => 'BVOWNER',
        policy_function => 'pol_hsba_func',
        statement_types => 'SELECT, UPDATE, DELETE',
        update_check    => TRUE
    );
END;
/

-- Áp dụng cho bảng BENHNHAN
BEGIN
    DBMS_RLS.ADD_POLICY(
        object_schema   => 'BVOWNER',
        object_name     => 'BENHNHAN',
        policy_name     => 'POLICY_BN_VPD',
        function_schema => 'BVOWNER',
        policy_function => 'pol_bn_func',
        statement_types => 'SELECT, UPDATE',
        update_check    => TRUE
    );
END;
/