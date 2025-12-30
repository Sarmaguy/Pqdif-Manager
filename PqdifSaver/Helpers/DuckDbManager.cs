using DuckDB.NET.Data;

public static class DuckDbManager
{
    /// <summary>
    /// Creates all required DuckDB tables if they do not already exist.
    /// </summary>

    public static void CreateTables()
    {
        string dbPath = ConfigBuilder.Instance.DuckDBConnectionString;
        using var connection = new DuckDBConnection(dbPath);
        connection.Open();

        using var cmd = connection.CreateCommand();

        // --- 1. Base Table ---
        cmd.CommandText = @"CREATE TABLE IF NOT EXISTS base (
            RecordingId SMALLINT NOT NULL,
            Time TIMESTAMP NOT NULL,
            U1 INTEGER, U2 INTEGER, U3 INTEGER,
            UN INTEGER, U12 INTEGER, U23 INTEGER, U31 INTEGER,
            I1 INTEGER, I2 INTEGER, I3 INTEGER, ""IN"" INTEGER,
            THD_U1 SMALLINT, THD_U2 SMALLINT, THD_U3 SMALLINT,
            THD_I1 SMALLINT, THD_I2 SMALLINT, THD_I3 SMALLINT,
            THD_UN SMALLINT, THD_IN SMALLINT,
            THD_U12 SMALLINT, THD_U23 SMALLINT, THD_U31 SMALLINT,
            PST1 SMALLINT, PST2 SMALLINT, PST3 SMALLINT,
            PST12 SMALLINT, PST23 SMALLINT, PST31 SMALLINT,
            PLT1 SMALLINT, PLT2 SMALLINT, PLT3 SMALLINT,
            PLT12 SMALLINT, PLT23 SMALLINT, PLT31 SMALLINT,
            PHI_U1 SMALLINT, PHI_U2 SMALLINT, PHI_U3 SMALLINT,
            PHI_I1 SMALLINT, PHI_I2 SMALLINT, PHI_I3 SMALLINT,
            PHI_UN SMALLINT, PHI_IN SMALLINT,
            PHI_U12 SMALLINT, PHI_U23 SMALLINT, PHI_U31 SMALLINT,
            P1 SMALLINT, P2 SMALLINT, P3 SMALLINT,
            PN SMALLINT, P SMALLINT,
            S1 SMALLINT, S2 SMALLINT, S3 SMALLINT,
            SN SMALLINT, S SMALLINT,
            Q1 SMALLINT, Q2 SMALLINT, Q3 SMALLINT,
            QN SMALLINT, Q SMALLINT,
            PF1 SMALLINT, PF2 SMALLINT, PF3 SMALLINT,
            PFN SMALLINT, PF SMALLINT,
            UU SMALLINT, UU0 SMALLINT,
            IU SMALLINT, IU0 SMALLINT,
            U1_MIN INTEGER, U2_MIN INTEGER, U3_MIN INTEGER,
            I1_MIN INTEGER, I2_MIN INTEGER, I3_MIN INTEGER,
            UN_MIN INTEGER, IN_MIN INTEGER,
            U12_MIN INTEGER, U23_MIN INTEGER, U31_MIN INTEGER,
            U1_MAX INTEGER, U2_MAX INTEGER, U3_MAX INTEGER,
            I1_MAX INTEGER, I2_MAX INTEGER, I3_MAX INTEGER,
            UN_MAX INTEGER, IN_MAX INTEGER,
            U12_MAX INTEGER, U23_MAX INTEGER, U31_MAX INTEGER,
            P1_MIN INTEGER, P2_MIN INTEGER, P3_MIN INTEGER,
            PN_MIN INTEGER, P_MIN INTEGER,
            P1_MAX INTEGER, P2_MAX INTEGER, P3_MAX INTEGER,
            PN_MAX INTEGER, P_MAX INTEGER,
            S1_MIN INTEGER, S2_MIN INTEGER, S3_MIN INTEGER,
            SN_MIN INTEGER, S_MIN INTEGER,
            S1_MAX INTEGER, S2_MAX INTEGER, S3_MAX INTEGER,
            SN_MAX INTEGER, S_MAX INTEGER,
            Q1_MIN INTEGER, Q2_MIN INTEGER, Q3_MIN INTEGER,
            QN_MIN INTEGER, Q_MIN INTEGER,
            Q1_MAX INTEGER, Q2_MAX INTEGER, Q3_MAX INTEGER,
            QN_MAX INTEGER, Q_MAX INTEGER
        );";
        cmd.ExecuteNonQuery();

        // --- 2. CurrentHarmonics Table ---
        cmd.CommandText = "CREATE TABLE IF NOT EXISTS CurrentHarmonics (RecordingId SMALLINT NOT NULL, TimeStamp TIMESTAMP NOT NULL, ";
        string[] harmonicPhases = { "I1H", "I2H", "I3H", "INH" };
        for (int p = 0; p < harmonicPhases.Length; p++)
        {
            for (int i = 0; i <= 63; i++)
            {
                cmd.CommandText += $"{harmonicPhases[p]}{i} SMALLINT";
                if (!(p == harmonicPhases.Length - 1 && i == 63)) cmd.CommandText += ", ";
            }
        }
        cmd.CommandText += ");";
        cmd.ExecuteNonQuery();

        // --- 3. CurrentInterharmonics Table ---
        cmd.CommandText = "CREATE TABLE IF NOT EXISTS CurrentInterharmonics (RecordingId SMALLINT NOT NULL, TimeStamp TIMESTAMP NOT NULL, ";
        string[] interharmonicsPhases = { "I1IH", "I2IH", "I3IH", "INIH" };
        for (int p = 0; p < interharmonicsPhases.Length; p++)
        {
            for (int i = 0; i <= 63; i++)
            {
                cmd.CommandText += $"{interharmonicsPhases[p]}{i} SMALLINT";
                if (!(p == interharmonicsPhases.Length - 1 && i == 63)) cmd.CommandText += ", ";
            }
        }
        cmd.CommandText += ");";
        cmd.ExecuteNonQuery();

        // --- 4. Frequency60Percentage Table ---
        cmd.CommandText = "CREATE TABLE IF NOT EXISTS Frequency60Percentage (RecordingId SMALLINT NOT NULL, TimeStamp TIMESTAMP NOT NULL, ";
        for (int i = 1; i <= 60; i++)
        {
            cmd.CommandText += $"Freq{i} INTEGER";
            if (i != 60) cmd.CommandText += ", ";
        }
        cmd.CommandText += ");";
        cmd.ExecuteNonQuery();


         // --- 5. trend Table ---
        cmd.CommandText = @"
        CREATE TABLE IF NOT EXISTS trend (
            RecordingId SMALLINT NOT NULL,
            Time TIMESTAMP NOT NULL,
            F INT NULL, F_MIN INT NULL, F_MAX INT NULL,
            U1 INT NULL, U2 INT NULL, U3 INT NULL,
            I1 INT NULL, I2 INT NULL, I3 INT NULL,
            UN INT NULL, ""IN"" INT NULL,
            U12 INT NULL, U23 INT NULL, U31 INT NULL,
            THD_U1 INT NULL, THD_U2 INT NULL, THD_U3 INT NULL,
            THD_I1 INT NULL, THD_I2 INT NULL, THD_I3 INT NULL,
            THD_UN INT NULL, THD_IN INT NULL,
            THD_U12 INT NULL, THD_U23 INT NULL, THD_U31 INT NULL,
            PST1 INT NULL, PST2 INT NULL, PST3 INT NULL,
            PST12 INT NULL, PST23 INT NULL, PST31 INT NULL,
            PLT1 INT NULL, PLT2 INT NULL, PLT3 INT NULL,
            PLT12 INT NULL, PLT23 INT NULL, PLT31 INT NULL,
            PHI_U1 INT NULL, PHI_U2 INT NULL, PHI_U3 INT NULL,
            PHI_I1 INT NULL, PHI_I2 INT NULL, PHI_I3 INT NULL,
            PHI_UN INT NULL, PHI_IN INT NULL,
            PHI_U12 INT NULL, PHI_U23 INT NULL, PHI_U31 INT NULL,
            P1 INT NULL, P2 INT NULL, P3 INT NULL, PN INT NULL, P INT NULL,
            S1 INT NULL, S2 INT NULL, S3 INT NULL, SN INT NULL, S INT NULL,
            Q1 INT NULL, Q2 INT NULL, Q3 INT NULL, QN INT NULL, Q INT NULL,
            PF1 INT NULL, PF2 INT NULL, PF3 INT NULL, PFN INT NULL, PF INT NULL,
            UU INT NULL,
            U1_MIN INT NULL, U2_MIN INT NULL, U3_MIN INT NULL,
            I1_MIN INT NULL, I2_MIN INT NULL, I3_MIN INT NULL,
            UN_MIN INT NULL, IN_MIN INT NULL,
            U12_MIN INT NULL, U23_MIN INT NULL, U31_MIN INT NULL,
            U1_MAX INT NULL, U2_MAX INT NULL, U3_MAX INT NULL,
            I1_MAX INT NULL, I2_MAX INT NULL, I3_MAX INT NULL,
            UN_MAX INT NULL, IN_MAX INT NULL,
            U12_MAX INT NULL, U23_MAX INT NULL, U31_MAX INT NULL,
            P1_MIN INT NULL, P2_MIN INT NULL, P3_MIN INT NULL, PN_MIN INT NULL, P_MIN INT NULL,
            P1_MAX INT NULL, P2_MAX INT NULL, P3_MAX INT NULL, PN_MAX INT NULL, P_MAX INT NULL,
            S1_MIN INT NULL, S2_MIN INT NULL, S3_MIN INT NULL, SN_MIN INT NULL, S_MIN INT NULL,
            S1_MAX INT NULL, S2_MAX INT NULL, S3_MAX INT NULL, SN_MAX INT NULL, S_MAX INT NULL,
            Q1_MIN INT NULL, Q2_MIN INT NULL, Q3_MIN INT NULL, QN_MIN INT NULL, Q_MIN INT NULL,
            Q1_MAX INT NULL, Q2_MAX INT NULL, Q3_MAX INT NULL, QN_MAX INT NULL, Q_MAX INT NULL,
            QF1 INT NULL, QF2 INT NULL, QF3 INT NULL, QF INT NULL,
            AN_IN1 INT NULL, UU0 INT NULL, IU INT NULL, IU0 INT NULL
        );";
        cmd.ExecuteNonQuery();

        // --- 6. VoltageHarmonics Table ---
        cmd.CommandText = "CREATE TABLE IF NOT EXISTS VoltageHarmonics (RecordingId SMALLINT NOT NULL, TimeStamp TIMESTAMP NOT NULL, ";
        
        string[] phases = { "U1", "U2", "U3", "UN", "U12", "U23", "U31" };
        foreach (var phase in phases)
        {
            int maxH = (phase.StartsWith("U1") || phase.StartsWith("U2") || phase.StartsWith("U3") || phase == "UN") ? 63 : 63;
            for (int h = 0; h <= maxH; h++)
            {
                string type = (h == 0 && (phase.StartsWith("U12") || phase.StartsWith("U23") || phase.StartsWith("U31"))) ? "INT" : "SMALLINT";
                cmd.CommandText += $"{phase}H{h} {type}";
                cmd.CommandText += (phase == "U31" && h == maxH) ? "" : ", ";
            }
        }
        cmd.CommandText += ");";
        cmd.ExecuteNonQuery();

        // --- 7. VoltageInterharmonics Table ---
        cmd.CommandText = "CREATE TABLE IF NOT EXISTS VoltageInterharmonics (RecordingId SMALLINT NOT NULL, TimeStamp TIMESTAMP NOT NULL, ";
        foreach (var phase in new string[] { "U1", "U2", "U3", "UN", "U12", "U23", "U31" })
        {
            for (int ih = 0; ih <= 49; ih++)
            {
                cmd.CommandText += $"{phase}IH{ih} SMALLINT";
                cmd.CommandText += (phase == "U31" && ih == 49) ? "" : ", ";
            }
        }
        cmd.CommandText += ");";
        cmd.ExecuteNonQuery();

        // --- 8. Events Table ---
        cmd.CommandText = 
        @"  CREATE TABLE PqEvents (
                TypeId      INTEGER NOT NULL,
                RecordingId INTEGER NOT NULL,
                StartTime   TIMESTAMP NOT NULL,
                EndTime     TIMESTAMP NOT NULL,

                -- Compressed waveform data (stored as binary blobs)
                Timestamp   BLOB NOT NULL,
                U1          BLOB,
                U2          BLOB,
                U3          BLOB,
                UN          BLOB,
                U12         BLOB,
                U23         BLOB,
                U31         BLOB,
                I1          BLOB,
                I2          BLOB,
                I3          BLOB,
                ""IN""          BLOB
            );";
            cmd.ExecuteNonQuery();

        Console.WriteLine("All DuckDB tables created successfully!");
    }
}