import uuid
from datetime import datetime

from pgvector.sqlalchemy import Vector
from sqlalchemy import (
    BigInteger,
    Computed,
    ForeignKey,
    Index,
    LargeBinary,
    SmallInteger,
    String,
    Text,
    UniqueConstraint,
    func,
    text,
)
from sqlalchemy.dialects.postgresql import ARRAY, JSONB, TSVECTOR, UUID
from sqlalchemy.orm import DeclarativeBase, Mapped, mapped_column, relationship

EMBEDDING_DIM = 384


class Base(DeclarativeBase):
    pass


class Source(Base):
    __tablename__ = "source"
    __table_args__ = (UniqueConstraint("kind", "company"),)

    id: Mapped[int] = mapped_column(SmallInteger, primary_key=True)
    kind: Mapped[str] = mapped_column(Text)
    company: Mapped[str] = mapped_column(Text)
    config: Mapped[dict] = mapped_column(JSONB, server_default=text("'{}'"))


class Job(Base):
    __tablename__ = "job"
    __table_args__ = (
        UniqueConstraint("source_id", "external_id"),
        Index(
            "job_open_filters",
            "remote_policy",
            "seniority",
            "years_required_min",
            postgresql_where=text("status = 'open'"),
        ),
        Index("job_tech_stack", "tech_stack", postgresql_using="gin"),
    )

    id: Mapped[int] = mapped_column(BigInteger, primary_key=True)
    source_id: Mapped[int] = mapped_column(ForeignKey("source.id"))
    external_id: Mapped[str] = mapped_column(Text)
    url: Mapped[str] = mapped_column(Text)
    title: Mapped[str] = mapped_column(Text)
    company: Mapped[str] = mapped_column(Text)
    location_raw: Mapped[str | None] = mapped_column(Text)
    description: Mapped[str] = mapped_column(Text)
    content_hash: Mapped[bytes] = mapped_column(LargeBinary)

    seniority: Mapped[str | None] = mapped_column(Text)
    years_required_min: Mapped[int | None] = mapped_column(SmallInteger)
    remote_policy: Mapped[str | None] = mapped_column(Text)
    office_city: Mapped[str | None] = mapped_column(Text)
    salary_min: Mapped[int | None]
    salary_max: Mapped[int | None]
    currency: Mapped[str | None] = mapped_column(String(3))
    contract_type: Mapped[str | None] = mapped_column(Text)
    tech_stack: Mapped[list[str]] = mapped_column(ARRAY(Text), server_default=text("'{}'"))
    required_skills: Mapped[list[str]] = mapped_column(ARRAY(Text), server_default=text("'{}'"))
    nice_to_have_skills: Mapped[list[str]] = mapped_column(
        ARRAY(Text), server_default=text("'{}'")
    )
    extraction_status: Mapped[str] = mapped_column(Text, server_default="pending")

    status: Mapped[str] = mapped_column(Text, server_default="open")
    first_seen_at: Mapped[datetime] = mapped_column(server_default=func.now())
    last_seen_at: Mapped[datetime] = mapped_column(server_default=func.now())

    chunks: Mapped[list["JobChunk"]] = relationship(
        back_populates="job", cascade="all, delete-orphan", order_by="JobChunk.ord"
    )


class JobChunk(Base):
    __tablename__ = "job_chunk"
    __table_args__ = (
        Index("job_chunk_job", "job_id"),
        Index(
            "job_chunk_embedding",
            "embedding",
            postgresql_using="hnsw",
            postgresql_ops={"embedding": "vector_cosine_ops"},
        ),
        Index("job_chunk_tsv", "tsv", postgresql_using="gin"),
    )

    id: Mapped[int] = mapped_column(BigInteger, primary_key=True)
    job_id: Mapped[int] = mapped_column(ForeignKey("job.id", ondelete="CASCADE"))
    ord: Mapped[int] = mapped_column(SmallInteger)
    section: Mapped[str | None] = mapped_column(Text)
    content: Mapped[str] = mapped_column(Text)
    embedding: Mapped[list[float]] = mapped_column(Vector(EMBEDDING_DIM))
    tsv: Mapped[str] = mapped_column(
        TSVECTOR, Computed("to_tsvector('simple', content)", persisted=True)
    )

    job: Mapped[Job] = relationship(back_populates="chunks")


class Cv(Base):
    __tablename__ = "cv"

    id: Mapped[uuid.UUID] = mapped_column(
        UUID(as_uuid=True), primary_key=True, server_default=func.gen_random_uuid()
    )
    raw_text: Mapped[str] = mapped_column(Text)
    profile: Mapped[dict] = mapped_column(JSONB)
    embedding: Mapped[list[float]] = mapped_column(Vector(EMBEDDING_DIM))
    created_at: Mapped[datetime] = mapped_column(server_default=func.now())


class Outbox(Base):
    __tablename__ = "outbox"
    __table_args__ = (
        Index("outbox_unpublished", "id", postgresql_where=text("published_at IS NULL")),
    )

    id: Mapped[int] = mapped_column(BigInteger, primary_key=True)
    topic: Mapped[str] = mapped_column(Text)
    payload: Mapped[dict] = mapped_column(JSONB)
    created_at: Mapped[datetime] = mapped_column(server_default=func.now())
    published_at: Mapped[datetime | None]
