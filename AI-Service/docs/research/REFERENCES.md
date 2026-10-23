# Biometric-Pet-Identification — References

Evidence base for [`LITERATURE_REVIEW.md`](LITERATURE_REVIEW.md). Numbered `[n]` as cited there.
⚠️ Captured via web search 2026-06-08/09 — **verify each DOI/venue before any publication or commercial
claim** (the skill's `verify_citations.py`/`parallel-cli` were unavailable in this environment).

## Animal / pet / person re-identification
1. *From species-specific models to universal re-ID: a survey of animal re-identification.* Information Fusion, 2025 (reviews 41 works, 2020–2025). ScienceDirect S1566253526002022.
2. Adam L. et al. *WildlifeReID-10k: Wildlife re-identification dataset with 10k individual animals.* arXiv:2406.09211.
3. *MetaWild: A Multimodal Dataset for Animal Re-Identification with Environmental Metadata.* ACM MM 2025.
4. *Toward re-identifying any animal.* NeurIPS 36 (2024).
5. Zheng L., Yang Y., Hauptmann A. *Person Re-identification: Past, Present and Future.* arXiv:1610.02984.
6. *Person Re-identification: A Retrospective on Domain Specific Open Challenges and Future Trends.* Pattern Recognition / arXiv:2202.13121 (230+ papers, 2015–21).
7. *PetFace: A Large-Scale Dataset and Benchmark for Animal Identification.* (257,484 individuals; seen re-ID + unseen verification). arXiv (PetFace).
8. *Deep Learning Pet Identification Using Face and Body.* Information (MDPI) 2023, 14(5):278.

## Dog nose-print biometrics
9. *Dog Identification System Using Nose Print Biometrics.* (ResearchGate 359274825).
10. *Dog Nose-Print Identification Using Deep Neural Networks (DNNet).* IEEE Access, 2021 (IEEE Xplore 9385064).
11. *DNNetV2 + CatFaceNet — Nose-to-ID lineage / unified dog-nose + cat-face framework* (Tiny-ViT + MagFace-based loss; ~99.8% Rank-1).
12. *Nose-to-ID: A Deep Learning Framework for Dog Identification Using Nose-Print Biometrics.* Guilan CSE, 2025 (YOLOv8 + ResNeSt, video).
13. *A Competitive Method for Dog Nose-print Re-identification.* arXiv:2205.15934.
14. *Dog nose-print recognition based on the shape and spatial features of scales.* Expert Systems with Applications, 2023 (S0957417423028105).
15. *Is a Dog's Nose Print Unique?* (popular-science corroboration of biometric uniqueness).

## Cat face recognition
16. Lin & Kuo. *Cat face recognition using deep learning.* 2018 (1,500 imgs / 150 cats; 94.1%).
17. *The Comparison of Individual Cat Recognition Using Neural Networks.* arXiv:2410.02305 (ConvNeXt/DenseNet > Siamese).
    + Kaggle *Cat Individual Images* dataset (timost1234/cat-individuals).

## Metric learning
18. Schroff F., Kalenichenko D., Philbin J. *FaceNet: A Unified Embedding for Face Recognition and Clustering.* CVPR 2015 / arXiv:1503.03832.
19. Deng J., Guo J., Xue N., Zafeiriou S. *ArcFace: Additive Angular Margin Loss for Deep Face Recognition.* CVPR 2019, pp.4690–4699 / arXiv:1801.07698 (~6,982 citations).
20. Meng Q., Zhao S., Huang Z., Zhou F. *MagFace: A Universal Representation for Face Recognition and Quality Assessment.* CVPR 2021 (Oral), pp.14225–14234 / arXiv:2103.06627.
21. *Deep Metric Learning: a (Long) Survey.* hav4ik.github.io (triplet/quadruplet/N-pair/lifted-structured; mining).

## Open-set recognition & OOD
22. *Managing the Unknown: a survey on Open Set Recognition and tangential areas.* arXiv:2312.08785 (2024).
23. Salehi M. et al. *A Unified Survey on Anomaly, Novelty, Open-Set, and Out-of-Distribution Detection.* arXiv 2021.
24. iCGY96. *awesome_OpenSetRecognition_list* (GitHub curated resource). + Scheirer et al. (foundational OSR, EVT).

## Efficient / lightweight models
25. *Lightweight Deep Learning for Resource-Constrained Environments: A Survey.* arXiv:2404.07236 (2024).
26. *Comparative Analysis of Lightweight Deep Learning Models for Memory-Constrained Devices.* arXiv:2505.03303 (2025). + Howard et al. *MobileNet* (2017); Tan & Le *EfficientNet* (2019).

## Deployment / Thai LMIC cautionary tale
27. Beede E. et al. *A Human-Centered Evaluation of a Deep Learning System Deployed in Clinics for the Detection of Diabetic Retinopathy.* (Google Health, Thailand). + arXiv:2012.01165.
28. *Real-time diabetic retinopathy screening by deep learning in a multisite national screening programme.* Lancet Digital Health, 2022; PIIS2589-7500(22)00017-6.

## Market / competitive (see docs/business/ for full treatment)
- Pet Tech Market (Arizton/ResearchAndMarkets): USD 7.63B (2024) → 17.25B (2030), CAGR 14.56%. GlobeNewswire 2025-05-06.
- Mordor Intelligence: Pet Tech USD 12.5B (2025) → 23.8B (2030), CAGR 13.87%.
- Grand View / Knowledge Sourcing / Deep Market Insights: Pet Wearables ~USD 3B (2025) → 5.5–9B (2030); identification & tracking ≈ 60% of segment.
- Thailand pet food: USD ~2.2–2.25B (2025); broader pet economy >250B baht; ~5.38M pets (3.45M dogs, 1.94M cats, 2025); 70%+ households; ฿41,100/pet/yr (Nation Thailand; USDA FAS TH2025-0029; Mordor; TGM Research).
- Competitors: **Petnow** (petnow.io — dog noseprint + cat face, B2B); **Alipay** pet-insurance face recognition (>99%, China; FindBiometrics); **NoseID** (Purina); TransUnion (pet fraud/identity).
- Bangkok pet microchip law (eff. 2026-01-10): Khaosod English; Bangkok Post; condodee. Pet microchip market: Coherent Market Insights; Business Research Insights.
