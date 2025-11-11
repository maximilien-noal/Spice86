#!/usr/bin/env python3
"""
Update performance charts from the SQLite database.
This script is run automatically by GitHub Actions when the master branch changes.
"""
import sqlite3
import matplotlib.pyplot as plt
import numpy as np
import sys
from pathlib import Path

# Paths
REPO_ROOT = Path(__file__).parent.parent.parent
DB_PATH = REPO_ROOT / "tests" / "PerformanceTests" / "Database" / "performance.db"
DOC_DIR = REPO_ROOT / "doc"
PERF_TESTS_DIR = REPO_ROOT / "tests" / "PerformanceTests"

# Test configuration
TEST_NAMES = {
    1: "Test 1: Arithmetic (10K)",
    2: "Test 2: Multiplication (5K)", 
    3: "Test 3: Division (3K)",
    4: "Test 4: Bit Ops (8K)",
    5: "Test 5: Loops (10K)"
}

TEST_COLORS = {
    1: '#4a9eff',
    2: '#2ecc71',
    3: '#e74c3c',
    4: '#f39c12',
    5: '#9b59b6'
}

def check_database_exists():
    """Check if the performance database exists."""
    if not DB_PATH.exists():
        print(f"Error: Database not found at {DB_PATH}")
        print("Performance tests must be run first to generate data.")
        return False
    return True

def get_recent_results(conn, limit=10):
    """Get recent test results from the database."""
    query = """
        SELECT 
            r.GitCommit,
            t.TestId,
            t.TestName,
            t.Cycles
        FROM PerformanceTestResults t
        INNER JOIN PerformanceTestRuns r ON t.RunId = r.Id
        WHERE r.GitCommit != ''
        ORDER BY r.RunTimestamp DESC
        LIMIT ?
    """
    cursor = conn.execute(query, (limit * 5,))  # limit * 5 tests
    return cursor.fetchall()

def organize_data(results):
    """Organize results by commit and test."""
    if not results:
        return {}, []
    
    # Group by commit
    commits_data = {}
    for git_commit, test_id, test_name, cycles in results:
        if git_commit not in commits_data:
            commits_data[git_commit] = {}
        commits_data[git_commit][test_id] = cycles
    
    # Get unique commits in order (most recent first)
    commits = []
    for git_commit in commits_data.keys():
        if git_commit not in commits:
            commits.append(git_commit)
    
    # Limit to recent commits with complete data (all 5 tests)
    complete_commits = []
    for commit in commits:
        if len(commits_data[commit]) == 5:
            complete_commits.append(commit)
        if len(complete_commits) >= 10:
            break
    
    return commits_data, complete_commits

def create_overview_chart(commits_data, commits):
    """Create the overview bar chart with all tests."""
    if len(commits) < 2:
        print("Not enough data for overview chart (need at least 2 commits)")
        return False
    
    fig, ax = plt.subplots(figsize=(12, 6))
    
    # Use up to 10 most recent commits
    recent_commits = commits[:min(10, len(commits))]
    x = np.arange(len(recent_commits))
    width = 0.15
    
    # Plot bars for each test
    for i, test_id in enumerate(range(1, 6)):
        data = [commits_data[commit].get(test_id, 0) for commit in recent_commits]
        offset = (i - 2) * width
        test_name = TEST_NAMES[test_id]
        short_name = test_name.split(':')[1].strip() if ':' in test_name else test_name
        bars = ax.bar(x + offset, data, width, 
                      label=short_name, color=TEST_COLORS[test_id], alpha=0.8)
    
    ax.set_xlabel('Commits (most recent on left)', fontsize=12, fontweight='bold')
    ax.set_ylabel('Cycles', fontsize=12, fontweight='bold')
    ax.set_title('Emulator Performance Across Recent Commits', fontsize=14, fontweight='bold')
    ax.set_xticks(x)
    
    # Show short commit hashes (7 chars)
    commit_labels = [c[:7] for c in recent_commits]
    ax.set_xticklabels(commit_labels, rotation=45, ha='right')
    
    ax.legend(loc='upper right', framealpha=0.9, fontsize=9)
    ax.grid(axis='y', alpha=0.3, linestyle='--')
    
    # Set y-axis to start from 0 and go slightly above max
    all_values = [commits_data[c].get(t, 0) for c in recent_commits for t in range(1, 6)]
    if all_values:
        ax.set_ylim(0, max(all_values) * 1.1)
    
    plt.tight_layout()
    output_path = DOC_DIR / 'perf_overview.png'
    plt.savefig(output_path, dpi=150, bbox_inches='tight', 
                facecolor='white', edgecolor='none')
    plt.close()
    print(f"Created: {output_path}")
    return True

def create_individual_chart(commits_data, commits, test_id, output_dir):
    """Create individual line chart for a specific test."""
    if len(commits) < 2:
        print(f"Not enough data for test {test_id} chart (need at least 2 commits)")
        return False
    
    fig, ax = plt.subplots(figsize=(10, 5))
    
    # Use up to 10 most recent commits
    recent_commits = commits[:min(10, len(commits))]
    x = np.arange(len(recent_commits))
    data = [commits_data[commit].get(test_id, 0) for commit in recent_commits]
    
    ax.plot(x, data, marker='o', linewidth=2, markersize=8, 
            color=TEST_COLORS[test_id], label=TEST_NAMES[test_id])
    
    ax.set_xlabel('Commits (most recent on left)', fontsize=12, fontweight='bold')
    ax.set_ylabel('Cycles', fontsize=12, fontweight='bold')
    
    test_name = TEST_NAMES[test_id]
    title_parts = test_name.split('(')
    if len(title_parts) == 2:
        ax.set_title(f"{title_parts[0].strip()}\n({title_parts[1]}", 
                     fontsize=14, fontweight='bold')
    else:
        ax.set_title(test_name, fontsize=14, fontweight='bold')
    
    ax.set_xticks(x)
    commit_labels = [c[:7] for c in recent_commits]
    ax.set_xticklabels(commit_labels, rotation=45, ha='right')
    
    # Set y-axis range with some padding
    if data:
        min_val = min(data)
        max_val = max(data)
        padding = (max_val - min_val) * 0.1 if max_val > min_val else max_val * 0.1
        ax.set_ylim(min_val - padding, max_val + padding)
    
    ax.grid(True, alpha=0.3, linestyle='--')
    
    plt.tight_layout()
    
    # Determine filename based on output directory
    if output_dir == DOC_DIR:
        filename = f'perf_test{test_id}_{test_name.split(":")[1].strip().split()[0].lower()}.png'
    else:
        filename = f'perf_trend_{test_name.split(":")[1].strip().split()[0].lower()}.png'
    
    output_path = output_dir / filename
    plt.savefig(output_path, dpi=150, bbox_inches='tight',
                facecolor='white', edgecolor='none')
    plt.close()
    print(f"Created: {output_path}")
    return True

def create_perf_tests_overview(commits_data, commits):
    """Create overview chart for PerformanceTests README."""
    if len(commits) < 2:
        print("Not enough data for PerformanceTests overview chart")
        return False
    
    fig, ax = plt.subplots(figsize=(12, 6))
    
    recent_commits = commits[:min(10, len(commits))]
    x = np.arange(len(recent_commits))
    width = 0.15
    
    for i, test_id in enumerate(range(1, 6)):
        data = [commits_data[commit].get(test_id, 0) for commit in recent_commits]
        offset = (i - 2) * width
        test_name = TEST_NAMES[test_id].split(':')[1].strip().replace(' (', ' ').replace(')', '')
        bars = ax.bar(x + offset, data, width, 
                      label=test_name, color=TEST_COLORS[test_id], alpha=0.8)
    
    ax.set_xlabel('Commits (most recent on left)', fontsize=12, fontweight='bold')
    ax.set_ylabel('Cycles', fontsize=12, fontweight='bold')
    ax.set_title('Performance Comparison Across All Tests', fontsize=14, fontweight='bold')
    ax.set_xticks(x)
    commit_labels = [c[:7] for c in recent_commits]
    ax.set_xticklabels(commit_labels, rotation=45, ha='right')
    ax.legend(loc='upper right', framealpha=0.9, fontsize=9)
    
    all_values = [commits_data[c].get(t, 0) for c in recent_commits for t in range(1, 6)]
    if all_values:
        ax.set_ylim(0, max(all_values) * 1.1)
    
    ax.grid(axis='y', alpha=0.3, linestyle='--')
    
    plt.tight_layout()
    output_path = PERF_TESTS_DIR / 'perf_comparison.png'
    plt.savefig(output_path, dpi=150, bbox_inches='tight',
                facecolor='white', edgecolor='none')
    plt.close()
    print(f"Created: {output_path}")
    return True

def main():
    """Main function to update all performance charts."""
    print("=" * 70)
    print("Updating Performance Charts from Database")
    print("=" * 70)
    
    # Check if database exists
    if not check_database_exists():
        print("\nNo database found. Creating placeholder charts with sample data...")
        # For now, just exit - the charts will be created when tests run
        print("Charts will be generated after performance tests run on master branch.")
        return 0
    
    # Connect to database
    try:
        conn = sqlite3.connect(DB_PATH)
        print(f"\nConnected to database: {DB_PATH}")
    except Exception as e:
        print(f"Error connecting to database: {e}")
        return 1
    
    try:
        # Get recent results
        results = get_recent_results(conn)
        if not results:
            print("\nNo data found in database.")
            print("Performance tests must be run first to generate data.")
            return 1
        
        print(f"Found {len(results)} result records")
        
        # Organize data
        commits_data, commits = organize_data(results)
        print(f"Found data for {len(commits)} commits with complete results")
        
        if len(commits) < 2:
            print("\nNeed at least 2 commits with complete data to generate charts.")
            print("Run performance tests on master branch to collect more data.")
            return 1
        
        # Create output directories
        DOC_DIR.mkdir(exist_ok=True)
        
        # Generate main README charts
        print("\n--- Generating Main README Charts ---")
        success = create_overview_chart(commits_data, commits)
        
        for test_id in range(1, 6):
            success = create_individual_chart(commits_data, commits, test_id, DOC_DIR) and success
        
        # Generate PerformanceTests README charts
        print("\n--- Generating PerformanceTests README Charts ---")
        success = create_perf_tests_overview(commits_data, commits) and success
        
        for test_id in range(1, 6):
            success = create_individual_chart(commits_data, commits, test_id, PERF_TESTS_DIR) and success
        
        if success:
            print("\n" + "=" * 70)
            print("All charts updated successfully!")
            print("=" * 70)
            return 0
        else:
            print("\nSome charts failed to generate.")
            return 1
            
    except Exception as e:
        print(f"\nError generating charts: {e}")
        import traceback
        traceback.print_exc()
        return 1
    finally:
        conn.close()

if __name__ == "__main__":
    sys.exit(main())
