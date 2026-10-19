import { useState, type ChangeEvent } from 'react';
import './App.css';

type Clause = {
    clauseId: string;
    sequenceIndex: number;
    sectionIdentifier: string;
    rawText: string;
    startPage: number;
    endPage: number;
};

type UploadResponse = {
    documentId: string;
    fileName: string;
    totalPages: number;
    totalClauses: number;
    fullText: string;
    clauses: Clause[];
};

type DebateArgument = {
    role: string;
    modelName?: string;
    argumentText: string;
    executionTimeMs: number;
};

type Verdict = {
    modelName?: string;
    riskScore: number;
    isContested: boolean;
    contestReason: string;
    verdictSummary: string;
    redlineRecommendation: string;
};

type DebateRecord = {
    defenderArgument: DebateArgument;
    auditorArgument: DebateArgument;
    verdict: Verdict;
};

type AuditRecord = {
    clauseId: string;
    sectionIdentifier: string;
    rawClauseText: string;
    debateRecord: DebateRecord;
};

type AuditBundle = {
    records: AuditRecord[];
};

function App() {
    const [selectedFile, setSelectedFile] = useState<File | null>(null);
    const [uploadedFile, setUploadedFile] = useState<File | null>(null);
    const [document, setDocument] = useState<UploadResponse | null>(null);
    const [auditBundle, setAuditBundle] = useState<AuditBundle | null>(null);
    const [error, setError] = useState('');
    const [isUploading, setIsUploading] = useState(false);
    const [isAuditing, setIsAuditing] = useState(false);
    const [defenderModel, setDefenderModel] = useState('phi3:mini');
    const [auditorModel, setAuditorModel] = useState('qwen2.5:1.5b');
    const [judgeModel, setJudgeModel] = useState('phi3:mini');

    function handleFileChange(event: ChangeEvent<HTMLInputElement>) {
        const file = event.target.files?.[0];
        const extension = file?.name.toLowerCase().split('.').pop();

        setSelectedFile(null);
        setUploadedFile(null);
        setDocument(null);
        setAuditBundle(null);
        setError('');

        if (!file) {
            return;
        }

        if (extension !== 'pdf') {
            setError('Only PDF files are allowed.');
            event.target.value = '';
            return;
        }

        setSelectedFile(file);
    }

    async function handleUpload() {
        if (!selectedFile || isUploading) {
            return;
        }

        const formData = new FormData();
        formData.append('file', selectedFile);
        setIsUploading(true);
        setError('');
        setDocument(null);
        setAuditBundle(null);

        try {
            const response = await fetch('/api/ContractAudit/upload', {
                method: 'POST',
                body: formData,
            });

            if (!response.ok) {
                throw new Error(await response.text() || 'The document could not be processed.');
            }

            const result = await response.json() as UploadResponse;
            setDocument(result);
            setUploadedFile(selectedFile);
        } catch (uploadError) {
            setError(uploadError instanceof Error ? uploadError.message : 'The document could not be processed.');
        } finally {
            setIsUploading(false);
        }
    }

    async function handleRunAudit() {
        if (!document || isAuditing) {
            return;
        }

        setIsAuditing(true);
        setError('');
        setAuditBundle(null);

        try {
            const response = await fetch(`/api/ContractAudit/documents/${document.documentId}/audit`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    defenderModel: defenderModel.trim() || 'phi3:mini',
                    auditorModel: auditorModel.trim() || 'qwen2.5:1.5b',
                    judgeModel: judgeModel.trim() || 'phi3:mini',
                }),
            });

            if (!response.ok) {
                throw new Error(await response.text() || 'The debate could not be completed.');
            }

            setAuditBundle(await response.json() as AuditBundle);
        } catch (auditError) {
            setError(auditError instanceof Error ? auditError.message : 'The debate could not be completed.');
        } finally {
            setIsAuditing(false);
        }
    }

    return (
        <main className="audit-page">
            <header className="page-header">
                <p className="eyebrow">BriefMind</p>
                <h1>Contract audit workspace</h1>
                <p>Upload a PDF to extract its content, review clauses, and run an AI debate.</p>
            </header>

            <section className="panel upload-panel" aria-labelledby="upload-title">
                <div className="panel-heading">
                    <div>
                        <p className="section-label">Step 1</p>
                        <h2 id="upload-title">Upload document</h2>
                    </div>
                    {uploadedFile && <span className="status-badge">Processed</span>}
                </div>
                <label className="file-picker">
                    <svg className="upload-document-icon" viewBox="0 0 48 48" aria-hidden="true">
                        <path d="M12 4h17l7 7v33H12z" />
                        <path d="M29 4v8h7M17 20h14M17 26h14M17 32h14M17 38h10" />
                    </svg>
                    <strong>{selectedFile?.name ?? 'Click to upload a PDF'}</strong>
                    <span>PDF files only</span>
                    <input type="file" accept=".pdf,application/pdf" onChange={handleFileChange} />
                </label>
                <button type="button" className="upload-button" disabled={!selectedFile || isUploading} onClick={handleUpload}>
                    {isUploading ? 'Processing...' : 'Upload Document'}
                </button>
            </section>

            {error && <p className="upload-error" role="alert">{error}</p>}

            {document && (
                <>
                    <section className="panel" aria-labelledby="content-title">
                        <div className="panel-heading">
                            <div>
                                <p className="section-label">Step 2</p>
                                <h2 id="content-title">Extracted PDF content</h2>
                            </div>
                            <span className="metadata">{document.totalPages} page{document.totalPages === 1 ? '' : 's'}</span>
                        </div>
                        <pre className="extracted-content">{document.fullText || 'No text was extracted from this PDF.'}</pre>
                    </section>

                    <section className="panel" aria-labelledby="clauses-title">
                        <div className="panel-heading">
                            <div>
                                <p className="section-label">Step 3</p>
                                <h2 id="clauses-title">Extracted clauses</h2>
                            </div>
                            <span className="metadata">{document.totalClauses} clause{document.totalClauses === 1 ? '' : 's'}</span>
                        </div>
                        <div className="clause-list">
                            {document.clauses.map((clause) => (
                                <article className="clause-card" key={clause.clauseId}>
                                    <div className="clause-heading">
                                        <strong>{clause.sectionIdentifier || `Clause ${clause.sequenceIndex}`}</strong>
                                        <span>Pages {clause.startPage}-{clause.endPage}</span>
                                    </div>
                                    <p>{clause.rawText}</p>
                                </article>
                            ))}
                        </div>
                    </section>

                    <section className="panel" aria-labelledby="debate-title">
                        <div className="panel-heading">
                            <div>
                                <p className="section-label">Step 4</p>
                                <h2 id="debate-title">Debate results</h2>
                            </div>
                            <button type="button" className="secondary-button" onClick={handleRunAudit} disabled={isAuditing}>
                                {isAuditing ? 'Running debate...' : 'Run debate'}
                            </button>
                        </div>
                        <div className="model-config-panel">
                            <h3 className="model-config-title">Model Configuration (RTX 1650 / 4GB VRAM Compatible)</h3>
                            <div className="model-config-grid">
                                <div className="model-field">
                                    <label htmlFor="defender-model">Defender Model</label>
                                    <select
                                        id="defender-model"
                                        value={defenderModel}
                                        onChange={(e) => setDefenderModel(e.target.value)}
                                        disabled={isAuditing}
                                    >
                                        <option value="phi3:mini">phi3:mini (Default Provider Defense)</option>
                                        <option value="qwen2.5:1.5b">qwen2.5:1.5b (~1.5 GB VRAM)</option>
                                        <option value="qwen2.5:3b">qwen2.5:3b (~2.5 GB VRAM)</option>
                                        <option value="qwen2.5:0.5b">qwen2.5:0.5b (Ultra-light)</option>
                                    </select>
                                </div>
                                <div className="model-field">
                                    <label htmlFor="auditor-model">Auditor Model (Adversarial Qwen)</label>
                                    <select
                                        id="auditor-model"
                                        value={auditorModel}
                                        onChange={(e) => setAuditorModel(e.target.value)}
                                        disabled={isAuditing}
                                    >
                                        <option value="qwen2.5:1.5b">qwen2.5:1.5b (Recommended for RTX 1650)</option>
                                        <option value="qwen2.5:3b">qwen2.5:3b (High Reasoning ~2.5GB)</option>
                                        <option value="qwen2.5:0.5b">qwen2.5:0.5b (Ultra-light ~0.6GB)</option>
                                        <option value="phi3:mini">phi3:mini (~2.3 GB VRAM)</option>
                                    </select>
                                </div>
                                <div className="model-field">
                                    <label htmlFor="judge-model">Judge Referee Model</label>
                                    <select
                                        id="judge-model"
                                        value={judgeModel}
                                        onChange={(e) => setJudgeModel(e.target.value)}
                                        disabled={isAuditing}
                                    >
                                        <option value="phi3:mini">phi3:mini (Default Referee)</option>
                                        <option value="qwen2.5:1.5b">qwen2.5:1.5b</option>
                                        <option value="qwen2.5:3b">qwen2.5:3b</option>
                                    </select>
                                </div>
                            </div>
                            <p className="model-vram-note">
                                💡 Sequential evaluation keeps VRAM footprint safely within 4 GB on NVIDIA GeForce GTX/RTX 1650.
                            </p>
                        </div>

                        {!auditBundle && <p className="empty-state">Run the debate to compare the defender and auditor arguments.</p>}
                        {auditBundle && (
                            <div className="debate-list">
                                {auditBundle.records.map((record) => (
                                    <article className="debate-card" key={record.clauseId}>
                                        <h3>{record.sectionIdentifier}</h3>
                                        <div className="debate-columns">
                                            <div>
                                                <span className="argument-label defender-label">
                                                    Defender
                                                    <span className="argument-meta">({record.debateRecord.defenderArgument.modelName || defenderModel})</span>
                                                </span>
                                                <p>{record.debateRecord.defenderArgument.argumentText}</p>
                                            </div>
                                            <div>
                                                <span className="argument-label auditor-label">
                                                    Auditor
                                                    <span className="argument-meta">({record.debateRecord.auditorArgument.modelName || auditorModel})</span>
                                                </span>
                                                <p>{record.debateRecord.auditorArgument.argumentText}</p>
                                            </div>
                                        </div>
                                        <div className="verdict">
                                            <div className="risk-score">
                                                Risk score: {record.debateRecord.verdict.riskScore}/100
                                                <span className="judge-tag">referee: {record.debateRecord.verdict.modelName || judgeModel}</span>
                                            </div>
                                            <p>{record.debateRecord.verdict.verdictSummary}</p>
                                            {record.debateRecord.verdict.isContested && <p><strong>Contested:</strong> {record.debateRecord.verdict.contestReason}</p>}
                                            {record.debateRecord.verdict.redlineRecommendation && <p><strong>Recommendation:</strong> {record.debateRecord.verdict.redlineRecommendation}</p>}
                                        </div>
                                    </article>
                                ))}
                            </div>
                        )}
                    </section>
                </>
            )}
        </main>
    );
}

export default App;