-- =========================================================================
-- YC2.sql - Yeu cau 2: OLS phat tan thong bao
-- =========================================================================

CONNECT BVOWNER/BVOwner#2026@localhost:1521/XEPDB1

-- ===================== LEVELS =====================
EXEC LBACSYS.SA_COMPONENTS.CREATE_LEVEL('BV_POLICY', 30, 'BGD', 'Ban Giam Doc');
EXEC LBACSYS.SA_COMPONENTS.CREATE_LEVEL('BV_POLICY', 20, 'LDK', 'Lanh Dao Khoa');
EXEC LBACSYS.SA_COMPONENTS.CREATE_LEVEL('BV_POLICY', 10, 'NV',  'Nhan Vien');

-- ===================== COMPARTMENTS (Khoa) =====================
EXEC LBACSYS.SA_COMPONENTS.CREATE_COMPARTMENT('BV_POLICY', 1, 'TH', 'Tieu Hoa');
EXEC LBACSYS.SA_COMPONENTS.CREATE_COMPARTMENT('BV_POLICY', 2, 'TK', 'Than Kinh');
EXEC LBACSYS.SA_COMPONENTS.CREATE_COMPARTMENT('BV_POLICY', 3, 'TM', 'Tim Mach');

-- ===================== GROUPS (Co so) =====================
-- Chỉ dùng 3 cơ sở: HCM / HP / HN.
-- Nếu cần quyền "tất cả cơ sở" thì gán label với group list: HCM,HP,HN (thay vì tạo group CSALL).
EXEC LBACSYS.SA_COMPONENTS.CREATE_GROUP('BV_POLICY', 11, 'HCM',   'Ho Chi Minh',  NULL);
EXEC LBACSYS.SA_COMPONENTS.CREATE_GROUP('BV_POLICY', 12, 'HP',    'Hai Phong',    NULL);
EXEC LBACSYS.SA_COMPONENTS.CREATE_GROUP('BV_POLICY', 13, 'HN',    'Ha Noi',       NULL);

-- ===================== TAO LABELS =====================
EXEC LBACSYS.SA_LABEL_ADMIN.CREATE_LABEL('BV_POLICY', 10000, 'NV',  TRUE);
EXEC LBACSYS.SA_LABEL_ADMIN.CREATE_LABEL('BV_POLICY', 30000, 'BGD', TRUE);
EXEC LBACSYS.SA_LABEL_ADMIN.CREATE_LABEL('BV_POLICY', 20000, 'LDK', TRUE);
EXEC LBACSYS.SA_LABEL_ADMIN.CREATE_LABEL('BV_POLICY', 20100, 'LDK:TH:HCM,HP,HN', TRUE);
EXEC LBACSYS.SA_LABEL_ADMIN.CREATE_LABEL('BV_POLICY', 10110, 'NV:TH:HCM', TRUE);
EXEC LBACSYS.SA_LABEL_ADMIN.CREATE_LABEL('BV_POLICY', 10130, 'NV:TH:HN', TRUE);
EXEC LBACSYS.SA_LABEL_ADMIN.CREATE_LABEL('BV_POLICY', 20210, 'LDK:TH,TK:HP', TRUE);


-- ===================== GAN LABEL CHO ADMIN =====================

BEGIN
    LBACSYS.SA_USER_ADMIN.SET_USER_LABELS(
        policy_name    => 'BV_POLICY',
        user_name      => 'BV_ADMIN',
        max_read_label => 'BGD:TH,TK,TM:HCM,HP,HN', -- Đọc tối đa (Full)
        max_write_label=> 'BGD:TH,TK,TM:HCM,HP,HN', -- Ghi tối đa (Full)
        min_write_label=> 'NV',                     
        def_label      => 'BGD:TH,TK,TM:HCM,HP,HN', -- Nhãn mặc định
        row_label      => 'BGD:TH,TK,TM:HCM,HP,HN'  -- Nhãn cho dòng mới
    );
END;
/
-- ===================== APPLY OLS POLICY LEN BANG THONGBAO =====================
-- Phai apply TRUOC khi update vi APPLY_TABLE_POLICY tu tao cot OLS_LABEL
BEGIN
    LBACSYS.LBAC_POLICY_ADMIN.APPLY_TABLE_POLICY(
        policy_name    => 'BV_POLICY',
        schema_name    => 'BVOWNER',
        table_name     => 'THONGBAO',
        table_options  => 'READ_CONTROL,LABEL_DEFAULT'
    );
END;
/

-- ===================== GAN OLS LABEL CHO DATA =====================
UPDATE BVOWNER.THONGBAO SET OLS_LABEL = 10000 WHERE MATB = 'TB_001';
UPDATE BVOWNER.THONGBAO SET OLS_LABEL = 30000 WHERE MATB = 'TB_002';
UPDATE BVOWNER.THONGBAO SET OLS_LABEL = 20000 WHERE MATB = 'TB_003';
UPDATE BVOWNER.THONGBAO SET OLS_LABEL = 20100 WHERE MATB = 'TB_004';
UPDATE BVOWNER.THONGBAO SET OLS_LABEL = 10110 WHERE MATB = 'TB_005';
UPDATE BVOWNER.THONGBAO SET OLS_LABEL = 10130 WHERE MATB = 'TB_006';
UPDATE BVOWNER.THONGBAO SET OLS_LABEL = 20210 WHERE MATB = 'TB_007';
COMMIT;

-- ===================== GAN LABEL CHO TAT CA NHAN VIEN =====================
DECLARE
    v_comp  VARCHAR2(20);
    v_grp   VARCHAR2(20);
    v_label VARCHAR2(100);

    CURSOR c_nv IS
        SELECT ORA_USERNAME, CHUYENKHOA, COSO
        FROM BVOWNER.NHANVIEN
        WHERE ORA_USERNAME IS NOT NULL;
BEGIN
    FOR r IN c_nv LOOP
        IF    r.CHUYENKHOA = 'Tieu hoa'  THEN v_comp := 'TH';
        ELSIF r.CHUYENKHOA = 'Than kinh' THEN v_comp := 'TK';
        ELSIF r.CHUYENKHOA = 'Tim mach'  THEN v_comp := 'TM';
        ELSE  v_comp := NULL;
        END IF;

        IF    r.COSO = 'TP HCM'    THEN v_grp := 'HCM';
        ELSIF r.COSO = 'Ha Noi'    THEN v_grp := 'HN';
        ELSIF r.COSO = 'Hai Phong' THEN v_grp := 'HP';
        ELSE  v_grp := 'HCM';
        END IF;

        IF v_comp IS NOT NULL THEN
            v_label := 'NV:' || v_comp || ':' || v_grp;
        ELSE
            v_label := 'NV::' || v_grp;
        END IF;

        LBACSYS.SA_USER_ADMIN.SET_USER_LABELS(
            policy_name    => 'BV_POLICY',
            user_name      => r.ORA_USERNAME,
            max_read_label => v_label,
            def_label      => v_label
        );
    END LOOP;
END;
/

PROMPT === YC2 DONE ===
