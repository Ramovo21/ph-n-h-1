-- =========================================================================
-- KICH BAN TEST YC2: ORACLE LABEL SECURITY (OLS)
-- Chay tung khoi trong SQL Developer
-- =========================================================================

-- =======================================================================
-- KB2.1: KIEM TRA CAU TRUC OLS DA TAO
-- Dang nhap: BV_ADMIN / BvAdmin@2026!
-- =======================================================================

-- Kiem tra OLS policy da duoc apply len THONGBAO
SELECT policy_name, schema_name, table_name, status
FROM dba_sa_table_policies
WHERE schema_name = 'BVOWNER';

-- Kiem tra cac LEVEL da tao
SELECT * FROM dba_sa_levels WHERE policy_name = 'BV_POLICY' ORDER BY level_num;
-- Ket qua mong doi: BGD(30), LDK(20), NV(10)

-- Kiem tra cac COMPARTMENT da tao
SELECT * FROM dba_sa_compartments WHERE policy_name = 'BV_POLICY' ORDER BY comp_num;
-- Ket qua mong doi: TH(1), TK(2), TM(3)

-- Kiem tra cac GROUP da tao
SELECT * FROM dba_sa_groups WHERE policy_name = 'BV_POLICY' ORDER BY group_num;
-- Ket qua mong doi: CSALL(10), HCM(11), HP(12), HN(13)

-- Kiem tra cac LABEL da tao
SELECT label_tag, label FROM dba_sa_labels WHERE policy_name = 'BV_POLICY' ORDER BY label_tag;
-- Ket qua mong doi: 20 labels
-- 8 labels tao thu cong:  NV(10001), NV:TH:HCM(10110), NV:TH:HN(10130),
--                         LDK::CSALL(20000), LDK(20001), LDK:TH:CSALL(20100),
--                         LDK:TH,TK:HP(20210), BGD::CSALL(30000)
-- 12 labels Oracle tu tao khi gan label cho user (1000000xxx):
--   VD: NV:TK:HN, NV:TM:HCM, NV::HCM, NV::HN... cho cac nhan vien
--       BGD:TH,TK,TM:CSALL cho BV_ADMIN va BVOWNER


-- =======================================================================
-- KB2.2: BV_ADMIN THAY TAT CA 7 THONG BAO
-- Dang nhap: BV_ADMIN / BvAdmin@2026!
-- =======================================================================

-- Nang session label len max de xem het 7 thong bao (def_label mac dinh BGD:TH,TK,TM:CSALL)
EXEC SA_SESSION.SET_LABEL('BV_POLICY', 'BGD:TH,TK,TM:CSALL');

SELECT t.MATB,
       l.LABEL                    AS LABEL_TEXT,
       SUBSTR(t.NOIDUNG, 1, 55)   AS NOIDUNG,
       SUBSTR(t.DIADIEM, 1, 40)   AS DIADIEM
FROM BVOWNER.THONGBAO t
LEFT JOIN DBA_SA_LABELS l ON l.LABEL_TAG   = t.OLS_LABEL
                          AND l.POLICY_NAME = 'BV_POLICY'
ORDER BY t.MATB;

-- Ket qua mong doi:
-- TB_001 | BGD::CSALL      | Hop khan ban giam doc             | Chi BGD thay
-- TB_002 | NV              | Cuoc hop toan bo nhan vien        | Tat ca thay
-- TB_003 | LDK             | Hop lanh dao cac khoa             | Chi LDK+ thay
-- TB_004 | LDK:TH:CSALL    | Hop lanh dao Khoa tieu hoa        | Chi LDK TH thay
-- TB_005 | NV:TH:HCM       | Cuoc hop NV Khoa tieu hoa tai HCM | NV TH HCM thay
-- TB_006 | NV:TH:HN        | Cuoc hop NV Khoa tieu hoa tai HN  | NV TH HN thay
-- TB_007 | LDK:TH,TK:HP    | Hop LD Tieu hoa + Than kinh HP    | Chi LDK TH,TK HP thay


-- =======================================================================
-- KB2.3: BS_001 (Tieu hoa, CS HCM) - Label: NV:TH:HCM
-- Dang nhap: BS_001 / BS@bv2026!
-- =======================================================================

SELECT MATB, SUBSTR(NOIDUNG, 1, 60) AS NOIDUNG
FROM BVOWNER.THONGBAO
ORDER BY MATB;

-- Ket qua mong doi: 2 dong
-- TB_002 | Cuoc hop toan bo nhan vien benh vien   (NV, khong comp/group -> ai cung thay)
-- TB_005 | Cuoc hop nhan vien Khoa tieu hoa tai HCM    (NV:TH:HCM -> khop voi BS_001)
--
-- KHONG thay: TB_001 (BGD > NV), TB_003 (LDK > NV), TB_004 (LDK > NV),
--             TB_006 (HN != HCM), TB_007 (LDK > NV)


-- =======================================================================
-- KB2.4: BS_002 (Than kinh, Ha Noi) - Label: NV:TK:HN
-- Dang nhap: BS_002 / BS@bv2026!
-- =======================================================================

SELECT MATB, SUBSTR(NOIDUNG, 1, 60) AS NOIDUNG
FROM BVOWNER.THONGBAO
ORDER BY MATB;

-- Ket qua mong doi: 1 dong
-- TB_002 | Cuoc hop toan bo nhan vien benh vien  (NV, khong comp -> ai cung thay)
--
-- KHONG thay: TB_005 (comp TH != TK), TB_006 (comp TH != TK)


-- =======================================================================
-- KB2.5: BS_007 (Tieu hoa, CS Ha Noi) - Label: NV:TH:HN
-- Dang nhap: BS_007 / BS@bv2026!
-- =======================================================================

SELECT MATB, SUBSTR(NOIDUNG, 1, 60) AS NOIDUNG
FROM BVOWNER.THONGBAO
ORDER BY MATB;

-- Ket qua mong doi: 2 dong
-- TB_002 | Cuoc hop toan bo nhan vien benh vien         (NV, khong comp/group)
-- TB_006 | Cuoc hop nhan vien Khoa tieu hoa tai Ha Noi  (NV:TH:HN -> khop)
--
-- KHONG thay TB_005 (NV:TH:HCM - group HCM != HN)


-- =======================================================================
-- KB2.6: BS_005 (Than kinh, CS Hai Phong) - Label: NV:TK:HP
-- Dang nhap: BS_005 / BS@bv2026!
-- =======================================================================

SELECT MATB, SUBSTR(NOIDUNG, 1, 60) AS NOIDUNG
FROM BVOWNER.THONGBAO
ORDER BY MATB;

-- Ket qua mong doi: 1 dong
-- TB_002 | Cuoc hop toan bo nhan vien benh vien  (NV, khong comp/group)
--
-- KHONG thay TB_007 (LDK:TH,TK:HP - level LDK > NV)


-- =======================================================================
-- KB2.7: KTV_001 (Xet nghiem, CS HCM) - Label: NV::HCM (khong co khoa)
-- Dang nhap: KTV_001 / KTV@bv2026!
-- =======================================================================

SELECT MATB, SUBSTR(NOIDUNG, 1, 60) AS NOIDUNG
FROM BVOWNER.THONGBAO
ORDER BY MATB;

-- Ket qua mong doi: 1 dong
-- TB_002 | Cuoc hop toan bo nhan vien benh vien  (NV, khong comp/group)
--
-- KHONG thay TB_005 (comp TH nhung KTV khong co comp -> khong du quyen)


-- =======================================================================
-- KB2.8: DPV_004 (Tieu hoa, CS Hai Phong) - Label: NV:TH:HP
-- Dang nhap: DPV_004 / DPV@bv2026!
-- =======================================================================

SELECT MATB, SUBSTR(NOIDUNG, 1, 60) AS NOIDUNG
FROM BVOWNER.THONGBAO
ORDER BY MATB;

-- Ket qua mong doi: 1 dong
-- TB_002 | Cuoc hop toan bo nhan vien benh vien  (NV, khong comp/group)
--
-- KHONG thay TB_007 (LDK:TH,TK:HP - level LDK > NV du cung HP)


-- =======================================================================
-- KB2.9: BN_001 KHONG DUOC XEM THONGBAO
-- Dang nhap: BN_001 / BN@bv2026!
-- =======================================================================

SELECT * FROM BVOWNER.THONGBAO;
-- Ket qua mong doi: ORA-00942 table or view does not exist
-- (Benh nhan khong co GRANT SELECT tren THONGBAO)
