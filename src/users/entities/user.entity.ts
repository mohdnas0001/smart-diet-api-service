import { Analysis } from '../../analysis/entities/analysis.entity';
import {
  Column,
  CreateDateColumn,
  Entity,
  OneToMany,
  PrimaryGeneratedColumn,
  UpdateDateColumn,
} from 'typeorm';

@Entity('users')
export class User {
  @PrimaryGeneratedColumn('uuid')
  id!: string;

  @Column({ length: 100 })
  name!: string;

  @Column({ unique: true })
  email!: string;

  @Column({ select: false })
  passwordHash!: string;

  @Column({ type: 'text', nullable: true, select: false })
  refreshTokenHash!: string | null;

  @Column({ type: 'int', nullable: true })
  age!: number | null;

  @Column({ type: 'double precision', nullable: true })
  weight!: number | null;

  @Column({ type: 'double precision', nullable: true })
  height!: number | null;

  @Column({ type: 'varchar', length: 20, nullable: true })
  gender!: string | null;

  @Column({ type: 'varchar', length: 30, nullable: true })
  activityLevel!: string | null;

  @Column({ type: 'varchar', length: 30, nullable: true })
  dietaryGoal!: string | null;

  @OneToMany(() => Analysis, (analysis) => analysis.user)
  analyses!: Analysis[];

  @CreateDateColumn()
  createdAt!: Date;

  @UpdateDateColumn()
  updatedAt!: Date;
}
