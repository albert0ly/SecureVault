# MedicalPoc01 POC

Minimal console POC that:
- Uses Azure Form Recognizer (Document Intelligence) to extract text from PDF medical documents (when `DOC_INTELLIGENCE_*` vars are set).
- Uses Azure OpenAI to evaluate whether recommended treatments/medications appear appropriate.

Environment variables required for full PDF -> evaluation flow:
- `DOC_INTELLIGENCE_ENDPOINT` — Form Recognizer endpoint
- `DOC_INTELLIGENCE_KEY` — Form Recognizer key
- `OPENAI_ENDPOINT` — Azure OpenAI endpoint (e.g. `https://your-resource.openai.azure.com`)
- `OPENAI_KEY` — Azure OpenAI key
- `OPENAI_DEPLOYMENT` — deployment name for the model

Run:
- dotnet build
- dotnet run --project MedicalPoc01

When prompted you may:
- Enter a path to a PDF file (will use Form Recognizer to extract text)
- Enter a path to a text file (plain .txt)
- Press Enter and paste text directly

Notes & cautions:
- This is a POC. Always have clinician verification. Do not use as an autonomous clinical decision system.
- Consider de-identifying PHI before sending to cloud. For production, add Key Vault, managed identity, private endpoints, audit logging, and BAA/HIPAA compliance.

Next steps (optional):
- Enhance entity extraction and structure medication/dosage detection.
- Improve prompt engineering and use chat completions API for conversational analysis.
- Add UI and logging for clinician feedback.
