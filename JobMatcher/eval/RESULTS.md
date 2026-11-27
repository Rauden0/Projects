# Evaluation results

Measured on 2026-11-27 on the development machine: RTX 3060 Ti (8 GB), Ollama in Docker
(`OLLAMA_CONTEXT_LENGTH=8192`, models at 100% GPU per `ollama ps`), embeddings
`intfloat/multilingual-e5-small` on CPU. Hosted comparison: `openai/gpt-oss-120b` on the Groq
free tier (the `CHAT_LLM_*` model from `.env`), paced to stay under 8,000 tokens per minute.

All three runners need the stack (`docker compose up -d postgres rabbitmq ollama`) and use
their own database, because they truncate it:

```bash
docker compose exec postgres psql -U jobmatcher -c "CREATE DATABASE jobmatcher_eval"
docker compose exec postgres psql -U jobmatcher -d jobmatcher_eval -c "CREATE EXTENSION vector"
DATABASE_URL=postgresql+asyncpg://jobmatcher:jobmatcher@localhost:5432/jobmatcher_eval alembic upgrade head
docker compose exec ollama ollama pull qwen2.5:7b      # and qwen2.5:3b, llama3.1:8b to compare
```

Raw predictions and transcripts are in `eval/extraction/results/` and `eval/agent/results/`.

## 1. Extraction

35 labelled postings in [`extraction/postings.yaml`](extraction/postings.yaml): 18 IT and
17 non-IT (driving, care, nursing, trades, office, retail, logistics, food, teaching), 16 in
Czech and 19 in English. Gold fields: seniority, minimum years, remote policy, office city,
salary min/max, currency, skills. A field may list several acceptable values where the ad is
honestly ambiguous (e.g. an on-site driving job that never says "on-site" accepts `onsite`
or `null`); Czech skills have Czech synonyms. Field accuracy counts `null` like any value.
Skills: recall over the gold list, precision over everything the model put into
tech + req + nice (precision is a lower bound: the gold lists are not exhaustive).

```bash
python eval/run_extraction.py --heuristic
python eval/run_extraction.py --model qwen2.5:7b                                   # production prompt (v2)
python eval/run_extraction.py --model qwen2.5:7b --prompt eval/extraction/prompts/v1-original.txt
python eval/run_extraction.py --model qwen2.5:7b --no-structured-output            # schema only in the prompt
python eval/run_extraction.py --model qwen2.5:3b
python eval/run_extraction.py --model llama3.1:8b
python eval/run_extraction.py --chat-model --reasoning-effort low --pace 12         # CHAT_LLM_* (Groq)
python eval/run_extraction.py --rescore eval/extraction/results/<run>.json         # re-score a saved run
```

### Summary

| run | mean field acc. | all 7 fields right | seniority | years | remote | city | salary min / max / cur | skills P / R / F1 | valid JSON 1st try / after retry | latency p50 / p95 | tokens in / out |
|---|---|---|---|---|---|---|---|---|---|---|---|
| heuristic (regex) | 59.2% | 11% | 94% | 97% | 60% | 80% | 26 / 43 / 14% | 1.00 / 0.27 / 0.42 | – | ~0 | 0 |
| qwen2.5:7b, v1 prompt, **before the currency fix** | 69.8% | 34% | 74% | 74% | 86% | 54% | 66 / 74 / 60% | 0.80 / 0.61 / 0.69 | 60% / 63% | 1.6 / 3.1 s | 934 / 166 |
| qwen2.5:7b, v1 prompt | 92.7% | 57% | 77% | 100% | 94% | 83% | 100 / 97 / 97% | 0.79 / 0.76 / 0.77 | 100% / 100% | 1.4 / 1.7 s | 658 / 119 |
| qwen2.5:7b, v1, schema in prompt only | 91.4% | 54% | 71% | 100% | 94% | 80% | 100 / 97 / 97% | 0.82 / 0.81 / 0.81 | 91% / 100% | 1.4 / 2.9 s | 720 / 133 |
| **qwen2.5:7b, v2 prompt (production)** | **95.5%** | **71%** | 91% | 100% | 94% | 91% | 97 / 97 / 97% | 0.82 / 0.88 / 0.85 | 100% / 100% | 1.4 / 1.6 s | 762 / 120 |
| qwen2.5:7b, v2, schema in prompt only | 92.7% | 63% | 86% | 97% | 91% | 89% | 97 / 94 / 94% | 0.82 / 0.86 / 0.84 | 91% / 97% | 1.4 / 3.2 s | 832 / 134 |
| qwen2.5:3b, v1 prompt | 77.1% | 17% | 26% | 97% | 60% | 83% | 91 / 91 / 91% | 0.59 / 0.63 / 0.61 | 100% / 100% | 0.8 / 1.1 s | 658 / 118 |
| qwen2.5:3b, v2 prompt | 77.6% | 14% | 31% | 80% | 86% | 86% | 86 / 80 / 94% | 0.74 / 0.77 / 0.76 | 100% / 100% | 0.9 / 1.1 s | 762 / 131 |
| llama3.1:8b, v2 prompt | 91.8% | 60% | 77% | 100% | 94% | 89% | 91 / 94 / 97% | 0.76 / 0.90 / 0.82 | 100% / 100% | 1.3 / 1.6 s | 743 / 108 |
| gpt-oss-120b (Groq), v2, reasoning low | 95.9% | 80% | 100% | 100% | 97% | 100% | 91 / 89 / 94% | 0.78 / 0.93 / 0.84 | 100% / 100% | 1.0 / 1.4 s³ | 1,084 / 268 |

³ Network latency to Groq at a paced 1 request per ~13 s; the free tier allows 8,000 tokens
per minute, i.e. about 5 postings a minute, so a crawl of a few hundred changed postings would
take an hour or need a paid tier.

### By subset (qwen2.5:7b, production prompt)

| subset | n | seniority | years | remote | city | salary min | salary max | currency | all fields | skills P / R / F1 |
|---|---|---|---|---|---|---|---|---|---|---|
| IT | 18 | 83% | 100% | 94% | 94% | 100% | 100% | 100% | 72% | 0.86 / 0.93 / 0.90 |
| non-IT | 17 | 100% | 100% | 94% | 88% | 94% | 94% | 94% | 71% | 0.73 / 0.75 / 0.74 |
| Czech | 16 | 81% | 100% | 88% | 94% | 94% | 94% | 94% | 50% | 0.87 / 0.83 / 0.85 |
| English | 19 | 100% | 100% | 100% | 89% | 100% | 100% | 100% | 89% | 0.79 / 0.90 / 0.84 |

### What the numbers changed

1. **A production bug: Czech salaries discarded the whole extraction.** qwen2.5:7b writes
   `"cur": "Kč"`; `JobFacts.currency` needs three letters, so validation failed, the retry
   failed the same way and the posting fell back to heuristic facts. 13 of the 16 Czech
   postings were affected (Czech all-fields-right: 0%). Fixed by mapping symbols to ISO codes
   and nulling anything else instead of failing (`CompactFacts._iso_currency`): valid output
   63% → 100%, mean field accuracy 69.8% → 92.7%.
2. **Prompt v2** ([`prompts/v2-explicit.txt`](extraction/prompts/v2-explicit.txt), now the
   production `EXTRACTION_SYSTEM`): seniority only when named (v1 said "from title or
   experience", and the model called most 3-year IT jobs senior: IT seniority 56% → 83%),
   city without district and null for remote jobs, skills as 1–4-word items in the posting's
   language including licences and certificates (non-IT skills F1 0.44 → 0.74). +100 input
   tokens per posting.
3. **`response_format` (constrained decoding) is worth keeping on for Ollama:** without it
   9% of first answers are not valid JSON (the retry recovers them, at the cost of a second
   call: p95 latency 1.6 s → 3.2 s) and accuracy drops 2.8 points.
4. **qwen2.5:3b is not good enough** for extraction: it labels seniority from experience
   regardless of the prompt (26–31%), and gets fewer than 1 in 6 postings fully right. It is
   1.6× faster, but extraction runs once per changed posting, so speed is not the constraint.
5. **Reasoning models need output headroom.** gpt-oss-120b reasons before answering; with the
   old 400-token cap the JSON was cut off in 94% of calls. The cap is now 1024 (the JSON is
   ~120 tokens, so other models are unaffected) and `LLM_REASONING_EFFORT` is passed through.

Remaining qwen2.5:7b errors are mostly honest ambiguity: an office city taken from a
"teambuilding in Brno" line of a remote job, `remote_region` for a Czech accounting job with
two home-office days, `salary_max` copied from a "from EUR 70,000" floor.

## 2. Retrieval

41 golden questions over 32 frozen postings: the original 25 IT questions plus 16 added ones
(non-IT occupations, Czech and English paraphrases, an English query for a Czech-only
posting, near neighbours such as truck vs. van driver and accountant vs. payroll accountant).

```bash
python eval/run_retrieval.py            # multilingual-e5-small
python eval/run_retrieval.py --hashing  # dependency-free baseline
```

| questions | mode | hit@5 | recall@10 | MRR |
|---|---|---|---|---|
| all (41) | vector | **0.85** | **0.90** | **0.81** |
| all (41) | full-text | 0.61 | 0.57 | 0.59 |
| all (41) | hybrid | 0.83 | 0.90 | 0.79 |
| original IT (25) | vector | 0.92 | 0.98 | 0.87 |
| original IT (25) | full-text | 0.68 | 0.65 | 0.64 |
| original IT (25) | hybrid | 0.88 | 0.97 | 0.83 |
| added (16) | vector | 0.75 | 0.78 | 0.73 |
| added (16) | full-text | 0.50 | 0.44 | 0.50 |
| added (16) | hybrid | 0.75 | 0.78 | 0.71 |
| all (41), hashing baseline | vector / full-text / hybrid | 0.59 / 0.61 / 0.63 | 0.65 / 0.57 / 0.64 | 0.56 / 0.59 / 0.59 |

- The original 25 questions score lower than in the README's earlier table (vector hit@5
  1.00 → 0.92) only because the corpus grew from 20 to 32 postings: the non-IT postings are
  new distractors.
- **Cross-language occupation queries are the weak spot.** All hybrid misses at k=5 except
  two are English questions for Czech-only postings ("nurse job in a hospital" →
  *Všeobecná sestra*, "payroll and HR administration" → *Mzdová účetní*, "manual work in a
  warehouse with a forklift" → *Skladník – VZV*) or abstract Czech paraphrases ("infrastruktura
  jako kód…", "propojování systémů přes API…"). e5-small does not bridge occupation
  vocabulary across languages, and full-text cannot.
- Heavier vector weights in the RRF (`WEIGHTS`: vector 2 or 3, or full-text 0.5) leave hit@5
  and recall unchanged and lower MRR (0.79 → 0.76), so equal weights stay. The gap is in the
  embeddings, not the fusion.

## 3. Agent

11 scripted questions ([`agent/cases.yaml`](agent/cases.yaml)) against the 32 fixtures seeded
with `https://jobs.example/<id>` URLs and one CV, through the production loop and tools, 3 runs
per case (sampling varies between runs). Columns: the expected tools were called; expected
filters were used; a relevant posting was cited (for "no job fits": nothing was cited);
citations valid = no URL in the answer that a tool did not return.

```bash
python eval/run_agent.py --model qwen2.5:7b                                         # production prompt (v2)
python eval/run_agent.py --model qwen2.5:7b --prompt eval/agent/prompts/v1-original.txt
python eval/run_agent.py --chat-model --reasoning-effort low --repeat 1 --pace 20   # CHAT_LLM_* (Groq)
```

| model, prompt | runs | expected tools | relevant cited | citations valid | mean rounds | latency p50 / max | tokens in / out per question | largest prompt |
|---|---|---|---|---|---|---|---|---|
| qwen2.5:7b, v1 prompt | 33 | 97% | 64% | 100% | 3.8 | 3.6 / 6.6 s | 5,409 / 207 | 3,237 |
| **qwen2.5:7b, v2 prompt (production)** | 33 | 97% | **85%** | 100%¹ | 3.0 | 3.8 / 7.7 s | 4,602 / 207 | 2,728 |
| qwen2.5:7b, v2, repeated after `OLLAMA_CONTEXT_LENGTH=8192` | 33 | 97% | 82% | 100% | 2.9 | 2.8 / 127 s² | 4,609 / 216 | 2,851 |
| qwen2.5:3b, v2 prompt | 33 | 88% | 67% | 100% | 2.2 | 2.0 / 46 s | 2,831 / 249 | 2,196 |
| llama3.1:8b, v2 prompt | 33 | 82% | 21% | 88% | 2.6 | 4.4 / 9.4 s | 2,413 / 217 | 1,720 |
| **gpt-oss-120b (Groq), v2, reasoning low** | 11⁴ | 100% | **100%** | 100% | 2.6 | 1.5 / 3.0 s | 3,292 / 226 | 2,079 |

² The 127 s turn is the first call after the Ollama container was recreated (model load);
the rest are under 10 s. 85% vs. 82% across the two v2 rows is run-to-run noise.

⁴ One run per case to stay inside the free tier's 8,000 tokens per minute (paced 20 s apart).

¹ One run answered in JSON and the URL check read the closing quote as part of each URL; that
was a bug in the check (fixed, `_URL_RE`), the citations themselves were right.

### Is qwen2.5:7b's tool use good enough?

Honestly: for single questions, mostly, after prompt work; it is not robust.

- With the original prompt it **guessed job ids** (`get_job(102)` for "the Golang Developer
  position" without searching), **stacked filters** (`city=Brno`, `remote_policy=[remote_global]`,
  `tech=[...]`, `max_years_required=5` for "remote Python roles"), searched only in English for
  Czech postings, and called `compare_with_cv` without a CV. It never invented a URL: the
  citation rule and the tool-result-only rule held in every run.
- Prompt v2 (search first, ids only from results, fewer filters, retry in Czech) fixed most of
  that: relevant postings cited 64% → 85%, one round less, 15% fewer tokens.
- What still fails: "remote Python roles that fit my CV" (0/3 with both prompts: it cites
  remote jobs, but not the two the gold set expects; the gold set may be strict here),
  occasionally recommending something for "no job fits" (1–2 of 3 runs), and run-to-run
  variance that a single run would hide.
- For comparison, **llama3.1:8b is unusable as the agent**: it invented URLs in 4 of 33 runs
  (`https://example.com/job1`, `https://www.jobserver.cz/jobs/1234`), printed tool calls as
  JSON text instead of calling the tool, and cited a relevant posting in 21% of runs. Those
  invented URLs are exactly what the runtime `unverified_urls` check reports.
- The hosted gpt-oss-120b got every case right in one run each, with fewer rounds and half
  the latency of the local model.
- qwen2.5:3b answers after one search almost always (2.2 rounds), skips `compare_with_cv`
  when asked for a gap analysis and is the only model with a pathological 46 s turn.

## 4. Recommendations

| Task | Recommendation |
|---|---|
| Extraction | Keep **qwen2.5:7b locally** with `response_format` on and prompt v2: 95.5% field accuracy, 100% valid JSON, 1.4 s per posting, no cost, no rate limit. gpt-oss-120b is equal on average (95.9%) and better on the hardest fields (all fields right 80% vs. 71%, seniority 100% vs. 91%), but the free tier's 8k tokens/minute caps it at ~5 postings a minute; worth it only on a paid tier or for `reextract` of the few postings that failed locally. Not qwen2.5:3b (77.6%) or llama3.1:8b (91.8%, seniority 77%). |
| Chat agent | Use the **hosted model for chat** (the current `.env`: `CHAT_LLM_*` = gpt-oss-120b on Groq, `LLM_REASONING_EFFORT=low`): 100% vs. 82–85% relevant answers, 1.5 s vs. 2.8–3.8 s. Keep qwen2.5:7b with prompt v2 as the local/offline fallback; it is usable but drops roughly one question in six. Do not run the agent on llama3.1:8b or qwen2.5:3b. Mind that chat sends the CV profile to the provider, and that the free tier (8k tokens/minute, ~3.3k tokens per question) serves about two questions a minute in total, so `CHAT_REQUESTS_PER_MINUTE` should stay low or the tier be upgraded. |
| Retrieval | Keep hybrid for the exact-token cases it was built for, but the measured weakness is cross-language occupation search. Cheapest fixes, in order: (1) let the agent search again in Czech (done in prompt v2: the nurse question went from 2/3 to 3/3); (2) index an English occupation title per posting (one more extraction key, embedded with the title chunk); (3) evaluate `multilingual-e5-base` (768 dims, needs a migration) or `bge-m3` on this golden set before switching. |
| Ollama | Keep `OLLAMA_CONTEXT_LENGTH=8192`: one agent question already reaches 3.2k prompt tokens, and the 4096 default silently cuts the start of the prompt (the system rules) in multi-turn chats. |
| Process | Run the extraction and agent evals before changing a prompt or model; both found production problems (the currency bug, the URL check) that unit tests did not. Agent results need repeated runs: identical runs of the v1 prompt scored 64% and 55%. |
| Schema | Add a pay period to the extraction schema (`per: hour | day | month | year`): hourly, daily and per-lesson rates are currently stored next to monthly salaries in the same column, which the salary filter cannot tell apart. |
